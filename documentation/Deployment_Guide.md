# Hướng dẫn Đóng gói & Cài đặt (Deployment Guide)

Tài liệu này hướng dẫn cách tạo file `.exe` (Requirement B7) và chạy trên máy khác.

---

## 1. Yêu cầu Môi trường
*   Windows 10 version 1809 trở lên hoặc Windows 11.
*   .NET 8.0/9.0 Runtimes (nếu đóng gói dạng Framework-dependent).

## 2. Cách Build ra file EXE (Publish)
Để nộp bài (thư mục Release), bạn cần thực hiện bước Publish:

1.  Mở Visual Studio 2022.
2.  Chuột phải vào Project `Project_MyShop_2025` -> Chọn **Publish**.
3.  Chọn **Folder** -> Next -> Chọn đường dẫn thư mục output (VD: `bin\Publish`).
4.  Tại màn hình Summary, bấm hình cái bút (Edit) ở phần **Settings**:
    *   **Configuration**: Release
    *   **Target Runtime**: `win-x64`
    *   **Deployment Mode**: `Self-contained` (Khuyên dùng - File nặng hơn nhưng chạy được trên máy không cài .NET) hoặc `Framework-dependent`.
    *   **File Publish Options**: Chọn "Produce single file" (Tạo ra 1 file exe duy nhất cho gọn).
5.  Bấm **Publish**.

## 3. Cấu trúc thư mục nộp bài
Theo yêu cầu nộp bài, bạn cần tổ chức folder như sau:

```
[MSSV1]_[MSSV2]_[MSSV3].zip
├── Source/                 # Code (đã xóa .vs, bin, obj)
├── Release/                # Kết quả Publish ở bước 2
│   ├── Project_MyShop_2025.exe
│   ├── Shop.db             # File database (nếu có sẵn dữ liệu mẫu)
│   └── resources.pri
└── readme.txt              # Thông tin nhóm
```

## 4. Troubleshooting (Sửa lỗi khi chạy)
*   **Lỗi không mở được Database**: Kiểm tra file `Shop.db` có nằm cùng thư mục với file `.exe` không. Nếu không, copy nó vào.
*   **Lỗi thiếu DLL**: Nếu build dạng "Framework-dependent", hãy chắc chắn máy giáo viên đã cài .NET Desktop Runtime. (Nên build Self-contained để tránh lỗi này).
*   **Lỗi đường dẫn ảnh**: Đảm bảo folder `Assets` được copy theo nếu ảnh không hiện.
