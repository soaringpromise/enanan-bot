# EnananBot

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet) ![NetCord](https://img.shields.io/badge/Discord-NetCord-5865F2?style=flat-square&logo=discord) ![SQLite](https://img.shields.io/badge/Database-SQLite-003B57?style=flat-square&logo=sqlite) ![License](https://img.shields.io/badge/License-MIT-blue?style=flat-square)

**EnananBot** is a feature-rich Discord bot designed primarily for **small servers**, focusing on **personal role management**, **color utilities**, and **social media link fixing**.

Built with the personality of **Ena Shinonome** from *Project SEKAI COLORFUL STAGE!*, the bot features immersive, randomized dialogue, custom emoji reactions, and artistic flair.

---

## Features

### Personal Role Management
Members can manage their own custom role without needing admin permissions.
* **Create:** `/role create [name] [color] (decorator)` – Creates a unique role for the user.
* **Edit:** `/role edit (name) (color) (decorator)` – Updates name or color.
* **Delete:** `/role delete` – Removes the role and cleans up the database.
* **Safety:** Adheres to Discord's 250-role limit per server.

### Color Utilities
Powered by **SkiaSharp** via **QuestPDF**, the bot generates real-time images to help visualize colors.
* **Preview:** `/color preview [color name/hex]` – Generates a fake Discord chat message to test readability.
* **Palette:** `/color palette [named color]` – Generates a 9-step gradient strip from an existing CSS color.
* **Validation:** Supports Hex (`#F00`, `#00FF00`, `00F`, `0F0F0F`) and 140+ named CSS colors (e.g., `HotPink`), all case-insensitive.

### Link Fixer
Automatically detects and fixes social media embeds:
* **Twitter/X** → `fxtwitter.com`
* **Reddit** → `fxreddit.com`
* **Pixiv** → `phixiv.net`
* **TikTok** → `vxtiktok.com`
* **Instagram** → `ddinstagram.com`
* *Spoiler support:* `||link||` remains spoilered after fix.

### Administration
* **Setup:** `/enanan setup` – Manually registers the guild and all non-bot members if automatic setup fails.
* **Reporting:** `/enanan list` – Generates a message or text file with all registered users and roles, sent via DM.
* **Welcome:** `/enanan welcome [channel]` – Designates a channel to send a welcome message for new members.

---

## Tech Stack

* **Framework:** [**.NET 10**](https://dotnet.microsoft.com/)
* **Library:** [**NetCord**](https://github.com/NetCordDev/NetCord) (Discord API wrapper)
* **Database:** **SQLite** (local file `EnananBot.db`)
  * Uses **WAL Mode** for high concurrency
* **Graphics:** [**QuestPDF**](https://www.questpdf.com/) (flexible layout engine, uses **SkiaSharp** internally)
* **Dependency Injection:** `Microsoft.Extensions.DependencyInjection`

---

## Installation & Setup

### Prerequisites
* **.NET 10 SDK** installed

### 1. Clone the Repository
```bash
    git clone https://github.com/soaringpromise/enanan-bot.git
    cd enanan-bot
```

### 2. Configuration (Environment Variables)

This bot reads its Discord token from an environment variable.

Set the following variable:
```bash
Discord__Token = YOUR_BOT_TOKEN

Windows (PowerShell):
$env:Discord__Token="YOUR_BOT_TOKEN"

Linux / macOS:
export Discord__Token="YOUR_BOT_TOKEN"
```

> The double underscore (__) maps to Discord:Token in .NET configuration.

### 3. Run the Bot (Local)

```bash
dotnet run --project EnananBot
```

> The database `EnananBot.db` should be created automatically on the first run.

---

## Command List

| Category  | Command           | Arguments                          | Description                                                                    |
|:----------|:------------------|:-----------------------------------|:-------------------------------------------------------------------------------|
| **Role**  | `/role create`    | `name`, `color`, `(decorator)`     | Creates a new custom role.                                                     |
|           | `/role edit`      | `(name)`, `(color)`, `(decorator)` | Updates an existing role (optional arguments allowed).                         |
|           | `/role delete`    | —                                  | Deletes your custom role.                                                      |
| **Color** | `/color preview`  | `color`                            | Generates a fake message to preview a color.                                   |
|           | `/color palette`  | `named color`                      | Generates a 9-step gradient palette image.                                     |
|           | `/color list`     | —                                  | Lists 148 supported CSS colors.                                                |
| **Admin** | `/enanan setup`   | —                                  | Manually registers the guild and all non-bot members if automatic setup fails. |
|           | `/enanan list`    | —                                  | Sends a DM report of all registered users and roles.                           |
|           | `/enanan welcome` | `channel`                          | Sets the channel for welcome messages.                                         |
| **Misc**  | `/credits`        | —                                  | Shows development and art credits.                                             |
|           | `/help [...]`     | —                                  | Shows all the commands and their explanations.                                 |
|           | `/donate`         | —                                  | Shows donation information.                                                    |
|           | `/invite`         | —                                  | Gives the bot's permanent invite link.                                         |

---

## Contributors

* **Programming:** [kii (me!) – @soaringpromise](https://x.com/soaringpromise) on Twitter/X.
* **Writing:** [RamblyngRobyn – @ramblyngrobyn.bsky.social](https://bsky.app/profile/ramblyngrobyn.bsky.social) on Bluesky.
* **Icon Art:** [Xin – @XinChan_](https://x.com/XinChan_) on Twitter/X.
* **Special Thanks:**
  * [enanan nation ♡ 絵名](<https://discord.gg/X7TBEFeQym>) Discord server.
  * My friends Ali and Konoha, for helping me test the features and remain sane during the development process.

---

## License
This project is licensed under the MIT License.