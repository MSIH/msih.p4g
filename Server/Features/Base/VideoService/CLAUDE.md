# VideoService

## Overview
The VideoService provides video processing and management capabilities specifically designed for campaign-related videos. It enables video uploads, referral code overlay processing, and video management operations. The service uses FFmpeg for video processing to add personalized referral codes as text overlays, creating customized videos for fundraiser campaigns.

## Architecture

### Components
- **VideoController**: RESTful API controller handling video operations
- **ProcessingJob**: Model representing background video processing tasks
- **VideoProcessRequest**: DTO for referral overlay processing requests
- **VideoUploadRequest**: DTO for video file uploads
- **ProcessingStatus**: Enum defining job processing states

### Dependencies
- **FFmpeg**: External video processing tool for adding text overlays
- **ASP.NET Core**: For file upload handling and API endpoints
- **Background Processing**: Task-based asynchronous video processing
- **File System**: Local file storage for video files

## Key Features
- Campaign video upload with file validation
- Asynchronous video processing with FFmpeg integration
- Referral code text overlay generation
- Background job status tracking and monitoring
- Video file management (list, delete, upload)
- Progress tracking for video processing operations
- Smart caching to avoid reprocessing unchanged videos

## API Endpoints

### Video Upload
**POST** `/api/video/upload`
- Uploads video files for campaigns
- Validates file type (mp4, mov, avi, mkv)
- Saves files with campaign ID as filename
- Returns upload confirmation with file details

### Add Referral Overlay
**POST** `/api/video/add-referral`
- Processes videos to add referral code overlays
- Creates background processing job
- Returns job ID for status tracking
- Queues FFmpeg processing with text overlay

### Job Status Tracking
**GET** `/api/video/status/{jobId}`
- Returns processing job status and progress
- Provides job details including completion time
- Shows error information if processing failed
- Returns output video URL when completed

### Video Management
**GET** `/api/video/list`
- Lists all uploaded campaign videos
- Shows file metadata (size, dates, campaign ID)
- Orders by last modified date
- Returns empty array if no videos exist

**GET** `/api/video/jobs`
- Lists all processing jobs with their status
- Shows job history and progress information
- Orders by creation date (newest first)
- Useful for monitoring system activity

**DELETE** `/api/video/delete/{campaignId}`
- Deletes video file for specific campaign
- Removes file from file system
- Returns confirmation of deletion
- Handles missing file gracefully

## Video Processing Workflow

### Upload Process
```csharp
// 1. File validation
var allowedExtensions = new[] { ".mp4", ".mov", ".avi", ".mkv" };
var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

// 2. Save to videos directory with campaign ID
var fileName = $"{campaignId}.mp4";
var filePath = Path.Combine("videos", fileName);

// 3. Store file to disk
await file.CopyToAsync(fileStream);
```

### Referral Processing
```csharp
// 1. Create processing job
var job = new ProcessingJob
{
    Id = Guid.NewGuid().ToString(),
    Status = ProcessingStatus.Queued,
    CreatedAt = DateTime.UtcNow
};

// 2. Start background processing
_ = Task.Run(() => ProcessVideoWithReferralAsync(referralCode, videoId, jobId));

// 3. FFmpeg command with text overlay
var ffmpegArgs = $"-i \"{inputPath}\" -vf \"drawtext=text='yoursite.com/ref={referralCode}':fontcolor=white:fontsize=24:x=(w-text_w)/2:y=h-50\" -c:a copy \"{outputPath}\"";
```

## Usage Examples

### Upload Video for Campaign
```javascript
// Frontend JavaScript example
const formData = new FormData();
formData.append('file', videoFile);
formData.append('campaignId', 123);

const response = await fetch('/api/video/upload', {
    method: 'POST',
    body: formData
});

const result = await response.json();
console.log('Video uploaded:', result.fileName);
```

### Process Video with Referral Code
```javascript
// Add referral code overlay to existing video
const response = await fetch('/api/video/add-referral', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
        referralCode: 'ABC123',
        videoId: 'campaign123'
    })
});

const result = await response.json();
const jobId = result.jobId;

// Poll for completion
const checkStatus = async () => {
    const statusResponse = await fetch(`/api/video/status/${jobId}`);
    const status = await statusResponse.json();
    
    if (status.status === 'completed') {
        console.log('Video ready:', status.videoUrl);
    } else if (status.status === 'failed') {
        console.error('Processing failed:', status.error);
    } else {
        console.log('Progress:', status.progress + '%');
        setTimeout(checkStatus, 2000); // Check again in 2 seconds
    }
};

checkStatus();
```

### List and Manage Videos
```javascript
// Get list of all videos
const videosResponse = await fetch('/api/video/list');
const videos = await videosResponse.json();

videos.forEach(video => {
    console.log(`Campaign ${video.campaignId}: ${video.fileName} (${video.fileSize} bytes)`);
});

// Delete a video
await fetch(`/api/video/delete/${campaignId}`, { method: 'DELETE' });
```

## Video Processing Details

### FFmpeg Integration
The service uses FFmpeg to add text overlays to videos:
- **Text Content**: Displays "yoursite.com/ref={referralCode}"
- **Position**: Centered horizontally, 50 pixels from bottom
- **Style**: White text, 24px font size
- **Audio**: Preserved without re-encoding (copy codec)

### Smart Caching
The system implements intelligent caching:
- Checks if processed video already exists
- Compares modification times of base and processed videos
- Skips processing if processed video is newer than base video
- Reprocesses only when base video has been updated

### Job Management
Processing jobs are tracked in memory with:
- **Unique Job IDs**: GUID-based identification
- **Status Tracking**: Queued → Processing → Completed/Failed
- **Progress Updates**: 0-100% completion tracking
- **Error Handling**: Detailed error messages on failure
- **Timestamps**: Creation and completion time tracking

## File Organization

### Directory Structure
```
Project Root/
├── videos/           # Original uploaded videos
│   ├── campaign1.mp4
│   ├── campaign2.mp4
│   └── ...
└── processed/        # Videos with referral overlays
    ├── ABC123_campaign1.mp4
    ├── XYZ789_campaign1.mp4
    └── ...
```

### Naming Convention
- **Original Videos**: `{campaignId}.mp4`
- **Processed Videos**: `{referralCode}_{videoId}.mp4`
- **Consistent Extensions**: All saved as .mp4 regardless of input format

## Error Handling

### Upload Validation
- File type validation for video formats only
- File size limits (handled by ASP.NET Core)
- Campaign ID validation requirements
- Directory creation for file storage

### Processing Errors
- Input file existence validation
- FFmpeg execution error handling
- Process exit code monitoring
- Detailed error logging and reporting

### System Requirements
- **FFmpeg**: Must be installed and accessible in system PATH
- **File System**: Write permissions for videos and processed directories
- **Memory**: Sufficient memory for concurrent video processing
- **CPU**: Processing power affects video conversion speed

## Integration Points

### Campaign Integration
- Videos are associated with campaign IDs
- Campaign-specific video storage and retrieval
- Integration with campaign management workflows

### Referral System Integration
- Referral codes embedded as video overlays
- Personalized videos for fundraiser sharing
- Links back to campaign donation pages

### File Management
- Local file system storage (can be extended to cloud storage)
- File cleanup and management capabilities
- Video metadata tracking and reporting

## Files

```
Server/Features/Base/VideoService/
├── Controllers/
│   └── VideoController.cs
└── CLAUDE.md
```