/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System.Collections.Generic;
using System.Linq;
using msih.p4g.Server.Features.Campaign.Data;
using CampaignModel = msih.p4g.Shared.Models.Campaign;

namespace msih.p4g.Server.Features.Campaign.Services
{
    /// <summary>
    /// EF Core service for managing donation campaigns.
    /// </summary>
    public class CampaignService
    {
        private readonly CampaignDbContext _db;
        public CampaignService(CampaignDbContext db)
        {
            _db = db;
        }

        public IEnumerable<CampaignModel> GetAll() => _db.Campaigns.Where(c => !c.IsDeleted).ToList();

        public CampaignModel GetById(int id) => _db.Campaigns.FirstOrDefault(c => c.Id == id && !c.IsDeleted);

        public CampaignModel Add(CampaignModel campaign)
        {
            _db.Campaigns.Add(campaign);
            _db.SaveChanges();
            return campaign;
        }

        public bool Update(CampaignModel updated)
        {
            var existing = _db.Campaigns.FirstOrDefault(c => c.Id == updated.Id && !c.IsDeleted);
            if (existing == null) return false;
            existing.Title = updated.Title;
            existing.Description = updated.Description;
            existing.IsActive = updated.IsActive;
            _db.SaveChanges();
            return true;
        }

        public bool Delete(int id)
        {
            var campaign = _db.Campaigns.FirstOrDefault(c => c.Id == id && !c.IsDeleted);
            if (campaign == null) return false;
            campaign.IsDeleted = true;
            _db.SaveChanges();
            return true;
        }
    }
}
