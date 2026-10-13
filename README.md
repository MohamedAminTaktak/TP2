# TP2

ASP.NET Core MVC e-commerce project for managing products, categories, orders and user authentication.

## Features
- Product catalog with category filtering and pagination
- Admin/Manager role-based access control
- Product and category CRUD
- Shopping cart and checkout flow
- ASP.NET Core Identity login and registration

## Startup notes
- The app seeds the `Admin` and `Manager` roles automatically at startup.
- A default admin user is also created automatically with these credentials:
  - Email: `admin@tp2.com`
  - Password: `Admin123!`

You can change the default admin credentials in `TP2/appsettings.json`.
