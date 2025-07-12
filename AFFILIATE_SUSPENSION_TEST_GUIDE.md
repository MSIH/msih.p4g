# Affiliate Suspension Feature - Integration Test Guide

## Overview
This document describes how to test the affiliate suspension feature that automatically suspends affiliate accounts when they abuse the system.

## Implementation Summary

### Database Changes
- Added 3 new fields to the `Profiles` table:
  - `IsSuspended` (bool, default false)
  - `SuspensionReason` (string, max 500 chars)
  - `SuspendedDate` (DateTime, nullable)

### Business Logic
The system now automatically checks for affiliate suspension when new donor accounts are created:

1. **First 2 accounts unqualified**: If the first two accounts linked to an affiliate have not made any donations, the affiliate is suspended.
2. **More than 9 unqualified accounts**: If more than 9 accounts linked to an affiliate have not made any donations, the affiliate is suspended.

### Email Notification
When an affiliate is suspended, an email notification is sent containing:
- Suspension reason
- Referral code
- Suspension date
- Contact information for support

## Testing Scenarios

### Scenario 1: First 2 Accounts Unqualified
1. Create an affiliate account (fundraiser) - note their referral code
2. Create 2 donor accounts using that referral code
3. Do NOT make any donations with these accounts
4. The affiliate should be automatically suspended and receive an email notification

### Scenario 2: More than 9 Unqualified Accounts
1. Create an affiliate account (fundraiser) - note their referral code
2. Create 10 donor accounts using that referral code
3. Do NOT make any donations with these accounts
4. The affiliate should be automatically suspended after the 10th account creation

### Scenario 3: Qualified Accounts (No Suspension)
1. Create an affiliate account (fundraiser) - note their referral code
2. Create 2+ donor accounts using that referral code
3. Make donations with at least some of these accounts
4. The affiliate should NOT be suspended

## Manual Testing Steps

### Using the Application UI:
1. Navigate to the user registration page
2. Create a fundraiser account
3. Note the referral code from the fundraiser's profile
4. Create multiple donor accounts using the referral code in the donation flow
5. Check the affiliate's profile to see if suspension occurred

### Database Verification:
```sql
-- Check affiliate suspension status
SELECT ReferralCode, IsSuspended, SuspensionReason, SuspendedDate 
FROM Profiles 
WHERE ReferralCode = 'AFFILIATE_CODE';

-- Count unqualified accounts for an affiliate
SELECT COUNT(*) as UnqualifiedCount
FROM Donors d
WHERE d.ReferralCode = 'AFFILIATE_CODE'
  AND d.IsActive = 1
  AND NOT EXISTS (
    SELECT 1 FROM Donations don 
    WHERE don.DonorId = d.Id
  );
```

## Key Integration Points

1. **DonationService.ProcessDonorRegistrationAsync()**: Checks suspension after new donor creation
2. **DonationService.ProcessDonationAsync()**: Checks suspension when creating donors during donation flow
3. **AffiliateMonitoringService**: Contains the core business logic for suspension checking
4. **Email notifications**: Automatic email sent via existing email service infrastructure

## Logging
The system logs all suspension activities with the following information:
- Affiliate referral code being checked
- Reason for suspension
- Email notification status
- Any errors during the process

Check application logs for entries containing "AffiliateMonitoringService" to trace the suspension process.

## Error Handling
- Affiliate monitoring errors do not break the donor creation process
- Failed email notifications are logged but don't prevent suspension
- All exceptions are caught and logged appropriately