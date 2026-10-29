# LocalBuzz 🐝

A local product discovery web application built with **ASP.NET Core (.NET 10)**, **C#**, **Entity Framework Core**, and **MySQL 8**. 

LocalBuzz enables customers to search for products locally (e.g., "wireless mouse" in Navsari), see which nearby brick-and-mortar shops have them in stock with live prices, compare options with lowest-price badges and distance calculations, and open one-click Google Maps turn-by-turn directions.

---

## Key Features

- **No Customer Accounts Needed**: Instant local discovery with geolocation support, radius filtering (2/5/10/25 km), and Indian Rupee (`₹`, `en-IN`) price comparison.
- **Shopkeeper Portal**: Self-registration, shop profile setup with working hours, open/closed indicator (IST `Asia/Kolkata` time with overnight support), and coordinates pinning.
- **Product & Inventory Management**: CRUD product listings, live In-Stock / Out-of-Stock toggles, and secure image uploads with magic-byte validation.
- **Zero External Map SDKs**: Native Google Maps directions URLs without requiring external map API keys.
- **Warm Minimalist Design**: Custom mobile-first design matching the reference design palette (cream background, dark ink borders, beige category tiles, orange accents).

---

## Demo Credentials (Seeded)

The database automatically seeds on first launch:

| Shop Name | Locality | Login Email | Password |
|---|---|---|---|
| Navsari Digital Hub | Station Road | `owner1@localbuzz.in` | `shop123` |
| Gadget Galaxy | Dudhia Talav | `owner2@localbuzz.in` | `shop123` |
| Shree Footwear | Vijalpore | `owner3@localbuzz.in` | `shop123` |
| Glow Care Store | Lunsikui | `owner4@localbuzz.in` | `shop123` |
| *(No Shop Yet)* | Onboarding Test | `newowner@localbuzz.in` | `shop123` |

Seeded products include **multi-shop price comparisons** (e.g. Logitech M170 Wireless Mouse at ₹399, ₹449, and ₹549) with lowest-price badges, out-of-stock variations, and Sunday weekly offs.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://docs.docker.com/get-docker/) & [Docker Compose](https://docs.docker.com/compose/)

---

## Quick Start

### 1. Start the MySQL 8 Database
```bash
docker compose up -d
```
This starts the MySQL container `lb-mysql` on port `3309` with database `localbuzz_db`.

### 2. Run the Application
```bash
dotnet run
```
On startup, the app automatically retries connecting to MySQL (up to ~60s), creates the database schema via `Database.EnsureCreated()`, and seeds the demo shopkeepers and catalogue.

Open your browser to:
```
http://localhost:5296
```

---

## Testing & Verifying Features

1. **Customer Search**:
   - Go to `/` (Home) or `/Search`.
   - Search for `wireless mouse`. You will see results from multiple shops with prices ranging from ₹399 to ₹549, with Gadget Galaxy marked with the **Lowest price** badge.
   - Click the orange **Location Pin** button to allow geolocation (or test with denied geolocation). Distances like `1.2 km` update dynamically.
   - Click **Directions** on any card to open Google Maps directions to that store.

2. **Shopkeeper Workflow**:
   - Click **For Shopkeepers** in the top navigation.
   - Sign in with `owner1@localbuzz.in` / `Password: shop123` (or register a brand-new shopkeeper).
   - In **Dashboard** (`/Dashboard`), update your shop details, operating hours, or toggle "Publish my shop in customer search results".
   - In **Products** (`/Products`), add a new product with an image, toggle stock status, or edit prices.
   - Ownership checks are strictly enforced server-side: shopkeepers cannot view, edit, or delete items belonging to another store.

---

## Uploads & Category Images

- **Product Uploads**: Uploaded product images are verified via file signature magic-bytes and stored at:
  ```
  wwwroot/uploads/products/{Guid}.{ext}
  ```
- **Category Tiles**: Category tiles check for custom images at:
  ```
  wwwroot/images/categories/{slug}.jpg
  ```
  (e.g., `electronics.jpg`, `footwear.jpg`, `personal-care.jpg`). If not present, the view automatically falls back to an emoji tile.

---

## Resetting the Database

To wipe the database and start fresh:
```bash
docker compose down -v
rm -rf wwwroot/uploads/products/*
dotnet run
```
