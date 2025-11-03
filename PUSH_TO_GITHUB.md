# Push to GitHub - Commands

After creating a GitHub repository, run these commands:

```bash
# Add remote (replace YOUR_USERNAME with your GitHub username)
git remote add origin https://github.com/YOUR_USERNAME/event-booking-system.git

# Rename branch to main (if needed)
git branch -M main

# Push to GitHub
git push -u origin main
```

If you haven't set up GitHub authentication, you'll need to:
- Use Personal Access Token (not password)
- Or set up SSH keys

## Quick Setup:
1. Create repo at: https://github.com/new
2. Name it: `event-booking-system`
3. Make it **Public**
4. Don't check any boxes (no README, no .gitignore)
5. Copy the commands from above and run them

