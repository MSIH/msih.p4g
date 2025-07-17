using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace msih.p4g.Server.Features.Base.VideoService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, ProcessingJob> _jobs = new();
        private readonly ILogger<VideoController> _logger;

        public VideoController(ILogger<VideoController> logger)
        {
            _logger = logger;
        }

        [HttpPost("add-referral")]
        public async Task<IActionResult> AddReferralUrl([FromBody] VideoProcessRequest request)
        {
            if (string.IsNullOrEmpty(request.ReferralCode) || string.IsNullOrEmpty(request.VideoId))
            {
                return BadRequest("ReferralCode and VideoId are required");
            }

            var jobId = Guid.NewGuid().ToString();
            var job = new ProcessingJob
            {
                Id = jobId,
                Status = ProcessingStatus.Queued,
                CreatedAt = DateTime.UtcNow
            };

            _jobs[jobId] = job;

            // Start processing in background
            _ = Task.Run(async () => await ProcessVideoWithReferralAsync(request.ReferralCode, request.VideoId, jobId));

            return Ok(new { jobId = jobId, status = "queued" });
        }

        [HttpGet("status/{jobId}")]
        public IActionResult GetStatus(string jobId)
        {
            if (!_jobs.TryGetValue(jobId, out var job))
            {
                return NotFound("Job not found");
            }

            return Ok(new 
            { 
                jobId = jobId,
                status = job.Status.ToString().ToLower(),
                progress = job.Progress,
                videoUrl = job.OutputPath,
                error = job.Error,
                createdAt = job.CreatedAt,
                completedAt = job.CompletedAt
            });
        }

        [HttpGet("jobs")]
        public IActionResult GetAllJobs()
        {
            var jobs = _jobs.Values.Select(j => new
            {
                jobId = j.Id,
                status = j.Status.ToString().ToLower(),
                progress = j.Progress,
                videoUrl = j.OutputPath,
                createdAt = j.CreatedAt,
                completedAt = j.CompletedAt
            }).OrderByDescending(j => j.createdAt);

            return Ok(jobs);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadVideo([FromForm] VideoUploadRequest request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            if (!request.CampaignId.HasValue)
            {
                return BadRequest("Campaign ID is required");
            }

            try
            {
                // Validate file type
                var allowedExtensions = new[] { ".mp4", ".mov", ".avi", ".mkv" };
                var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Invalid file type. Only video files are allowed.");
                }

                // Create videos directory if it doesn't exist
                var videosDirectory = Path.Combine(Directory.GetCurrentDirectory(), "videos");
                Directory.CreateDirectory(videosDirectory);

                // Save file with campaign ID as filename
                var fileName = $"{request.CampaignId}.mp4";
                var filePath = Path.Combine(videosDirectory, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                _logger.LogInformation($"Video uploaded successfully for campaign {request.CampaignId}");

                return Ok(new { 
                    message = "Video uploaded successfully", 
                    campaignId = request.CampaignId,
                    fileName = fileName,
                    filePath = filePath
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading video");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("list")]
        public IActionResult ListVideos()
        {
            try
            {
                var videosDirectory = Path.Combine(Directory.GetCurrentDirectory(), "videos");
                
                if (!Directory.Exists(videosDirectory))
                {
                    return Ok(new List<object>());
                }

                var videoFiles = Directory.GetFiles(videosDirectory, "*.mp4")
                    .Select(filePath => new
                    {
                        fileName = Path.GetFileName(filePath),
                        campaignId = Path.GetFileNameWithoutExtension(filePath),
                        filePath = filePath,
                        fileSize = new FileInfo(filePath).Length,
                        createdAt = System.IO.File.GetCreationTime(filePath),
                        lastModified = System.IO.File.GetLastWriteTime(filePath)
                    })
                    .OrderByDescending(f => f.lastModified)
                    .ToList();

                return Ok(videoFiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing videos");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("delete/{campaignId}")]
        public IActionResult DeleteVideo(string campaignId)
        {
            try
            {
                var videosDirectory = Path.Combine(Directory.GetCurrentDirectory(), "videos");
                var filePath = Path.Combine(videosDirectory, $"{campaignId}.mp4");

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    _logger.LogInformation($"Video deleted for campaign {campaignId}");
                    return Ok(new { message = "Video deleted successfully" });
                }

                return NotFound("Video not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting video for campaign {campaignId}");
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task ProcessVideoWithReferralAsync(string referralCode, string videoId, string jobId)
        {
            try
            {
                var job = _jobs[jobId];
                job.Status = ProcessingStatus.Processing;
                job.Progress = 0;

                var inputPath = Path.Combine("videos", $"{videoId}.mp4");
                var outputPath = Path.Combine("processed", $"{referralCode}_{videoId}.mp4");

                // Ensure output directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                // Validate input file exists
                if (!System.IO.File.Exists(inputPath))
                {
                    job.Status = ProcessingStatus.Failed;
                    job.Error = "Input video file not found";
                    return;
                }

                // Check if processed video exists and is newer than base video
                if (System.IO.File.Exists(outputPath))
                {
                    var baseVideoModified = System.IO.File.GetLastWriteTime(inputPath);
                    var processedVideoModified = System.IO.File.GetLastWriteTime(outputPath);
                    
                    if (processedVideoModified >= baseVideoModified)
                    {
                        job.Status = ProcessingStatus.Completed;
                        job.Progress = 100;
                        job.OutputPath = outputPath;
                        job.CompletedAt = DateTime.UtcNow;
                        _logger.LogInformation($"Using existing processed video for job {jobId} (base video unchanged)");
                        return;
                    }
                    else
                    {
                        _logger.LogInformation($"Base video newer than processed video for job {jobId} - reprocessing");
                    }
                }

                job.Progress = 10;

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = $"-i \"{inputPath}\" -vf \"drawtext=text='yoursite.com/ref={referralCode}':fontcolor=white:fontsize=24:x=(w-text_w)/2:y=h-50\" -c:a copy \"{outputPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                _logger.LogInformation($"Starting video processing for job {jobId}");
                
                var errorOutput = string.Empty;
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        errorOutput += e.Data + "\n";
                        // Parse ffmpeg progress if needed
                        if (e.Data.Contains("time="))
                        {
                            job.Progress = Math.Min(90, job.Progress + 10);
                        }
                    }
                };

                process.Start();
                process.BeginErrorReadLine();
                
                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    job.Status = ProcessingStatus.Completed;
                    job.Progress = 100;
                    job.OutputPath = outputPath;
                    job.CompletedAt = DateTime.UtcNow;
                    _logger.LogInformation($"Video processing completed for job {jobId}");
                }
                else
                {
                    job.Status = ProcessingStatus.Failed;
                    job.Error = $"FFmpeg failed with exit code {process.ExitCode}: {errorOutput}";
                    _logger.LogError($"Video processing failed for job {jobId}: {job.Error}");
                }
            }
            catch (Exception ex)
            {
                var job = _jobs[jobId];
                job.Status = ProcessingStatus.Failed;
                job.Error = ex.Message;
                _logger.LogError(ex, $"Video processing error for job {jobId}");
            }
        }
    }

    public class VideoProcessRequest
    {
        public string ReferralCode { get; set; }
        public string VideoId { get; set; }
    }

    public class VideoUploadRequest
    {
        public IFormFile File { get; set; }
        public int? CampaignId { get; set; }
    }

    public class ProcessingJob
    {
        public string Id { get; set; }
        public ProcessingStatus Status { get; set; }
        public int Progress { get; set; }
        public string OutputPath { get; set; }
        public string Error { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public enum ProcessingStatus
    {
        Queued,
        Processing,
        Completed,
        Failed
    }
}