# 🛍️ MyShop 2025 - Modern Retail Management System

![Version](https://img.shields.io/badge/version-1.0.0-blue.svg) ![Platform](https://img.shields.io/badge/platform-Windows_11_%7C_10-0078D7.svg) ![Tech](https://img.shields.io/badge/.NET-9.0-512BD4.svg)

> **A professional, high-performance Point of Sale (POS) and Inventory Management system built with WinUI 3 and Clean Architecture.**

---

## ✨ Outstanding Features (10/10 Score Targets)

This project goes beyond the basics to deliver a premium experience:

*   **🧠 AI-Powered Assistant**: Integrated **Gemini AI** for intelligent product descriptions and business insights.
*   **📊 Interactive Dashboard**: Real-time analytics, revenue charts, and "Best Sellers" visualization.
*   **📱 Modern UI/UX**: Fluent Design, dark/light mode support, and smooth animations using WinUI 3.
*   **📦 Smart Inventory**: Import products from Excel/Access, detailed categorization, and auto-generated SKU/Barcodes.
*   **🛒 Robust Order Processing**: Creating orders, printing invoices (PDF/XPS), and tracking order status (New, Shipped, Cancelled).
*   **📉 Dynamic Reporting**: Advanced filtering, sorting, and pagination for thousands of records.
*   **🛡️ Enterprise-Grade Reliability**:
    *   **Auto-Save Drafts**: Never lose work when creating orders/products.
    *   **Backup & Restore**: specialized database management.
    *   **Secure Authentication**: SHA256 password hashing.

---

## 🛠️ Tech Stack

*   **Framework**: WinUI 3 (Windows App SDK 1.5)
*   **Core**: .NET 9.0
*   **Database**: SQLite (w/ Entity Framework Core 9)
*   **Architecture**: MVVM (Model-View-ViewModel)
*   **Libraries**:
    *   `CommunityToolkit.Mvvm` - Efficient MVVM pattern
    *   `LiveChartsCore.SkiaSharp` - Beautiful charts
    *   `MiniExcel` - High-performance Excel processing
    *   `Mapster` - Object mapping
    *   `Google.Cloud.AIPlatform` - Generative AI

---

## 🚀 Getting Started

### Prerequisites
*   Windows 10 version 1903 (Build 18362) or newer.
*   [.NET 9.0 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) (if running portable).

### Installation

**Option 1: Installer (Recommended)**
1. Go to the `Release/Installer` folder.
2. Run `MyShop2025_Setup_1.0.0.exe`.
3. Follow the installation wizard.

**Option 2: Portable Mode**
1. Go to `Release/Portable`.
2. Run `Project_MyShop_2025.exe`.
3. The database `myshop.db` will be automatically created and seeded with 66 sample products.

---

## 📂 Project Structure

```mermaid
graph TD
    Root[Solution] --> Core[Project_MyShop_2025.Core]
    Root --> UI[Project_MyShop_2025 (UI)]
    Root --> Tests[Project_MyShop_2025.Tests]

    Core --> Models
    Core --> Services[Services & Interfaces]
    Core --> Data[Database Context & Seeder]

    UI --> ViewModels
    UI --> Views[Pages & Windows]
    UI --> Services[UI Services - Nav, Dialog]
    
    Tests --> UnitTests
```

---

## 📚 Documentation
Detailed documentation is available in the `documentation/` folder:

*   [🏗️ Architecture & Design](documentation/Architecture_Design.md)
*   [🔧 Key Technical Features](documentation/Key_Features_Technical.md)
*   [🗃️ Database Schema](documentation/Database_Schema.md)
*   [🎓 Viva Q&A Prep](documentation/Viva_Prep_QA.md)

---

## 👨‍💻 Author
**HCMUS Student** - Class 2025
*Project for "Windows Programming" course.*