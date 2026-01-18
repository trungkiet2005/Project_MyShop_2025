# Hướng Dẫn Đóng Gói & Cài Đặt MyShop 2025

## 📦 Cách 1: Portable (Đơn giản nhất)

1. Build project bằng Visual Studio hoặc lệnh:
   ```powershell
   dotnet build
   ```

2. Copy toàn bộ thư mục output:
   ```
   Project_MyShop_2025\bin\Debug\net9.0-windows10.0.19041.0\win-x64\
   ```

3. Đặt thư mục này vào máy khác và chạy `Project_MyShop_2025.exe`

---

## 📦 Cách 2: Tạo Installer bằng Inno Setup

### Bước 1: Cài đặt Inno Setup

1. Tải Inno Setup miễn phí từ: https://jrsoftware.org/isdl.php
2. Chạy file cài đặt và làm theo hướng dẫn

### Bước 2: Build Project

Chạy PowerShell script để build:

```powershell
.\build_release.ps1
```

Hoặc build thủ công:

```powershell
dotnet build --configuration Debug
```

### Bước 3: Tạo Installer

1. Mở file `installer.iss` bằng Inno Setup Compiler
2. Nhấn **Ctrl+F9** hoặc menu **Build > Compile**
3. File installer sẽ được tạo tại: `Release\Installer\MyShop2025_Setup_1.0.0.exe`

### Bước 4: Phân phối

- Gửi file `MyShop2025_Setup_1.0.0.exe` cho người dùng
- Họ chỉ cần chạy file này để cài đặt ứng dụng

---

## 📦 Cách 3: Tạo MSIX Package (Cho Windows Store)

WinUI 3 hỗ trợ đóng gói MSIX. Để tạo package:

1. Right-click vào project trong Visual Studio
2. Chọn **Publish** > **Create App Packages**
3. Chọn **Sideloading** 
4. Chọn **Create packages** và làm theo wizard

---

## 📁 Cấu Trúc Thư Mục Release

```
Release/
├── Portable/                    # Bản chạy trực tiếp
│   ├── Project_MyShop_2025.exe
│   ├── myshop.db               # Database SQLite
│   └── ...                     # Các file DLL khác
│
└── Installer/                   # Bản cài đặt
    └── MyShop2025_Setup_1.0.0.exe
```

---

## ⚙️ Yêu Cầu Hệ Thống

- **Hệ điều hành**: Windows 10 version 1903 trở lên (64-bit)
- **Runtime**: .NET 9.0 (sẽ được yêu cầu cài khi chạy nếu chưa có)
- **RAM**: Tối thiểu 4GB
- **Disk**: 200MB trống

---

## 🔧 Khắc Phục Lỗi Thường Gặp

### Lỗi "App can't open"
- Đảm bảo chạy trên Windows 10 1903+ 64-bit
- Cài .NET 9.0 Desktop Runtime

### Lỗi DLL missing
- Kiểm tra đã copy toàn bộ thư mục output
- Chạy `dotnet build` lại để đảm bảo đầy đủ dependencies

### Database không tạo được
- Kiểm tra quyền ghi vào thư mục chứa ứng dụng
- Chạy ứng dụng với quyền Administrator

---

## 📝 Ghi Chú Quan Trọng

1. **Đổi version**: Sửa `#define MyAppVersion "1.0.0"` trong file `installer.iss`
2. **Thêm icon**: Uncomment dòng `SetupIconFile` trong `installer.iss` và đặt đúng đường dẫn
3. **Với VIVA**: Demo cách chạy installer và cài đặt trên máy khác

---

## 👤 Tác Giả

HCMUS Student Project - 2025
