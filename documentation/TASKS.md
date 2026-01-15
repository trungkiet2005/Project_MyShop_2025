# MyShop 2025 - Part C Implementation Tasks

## Overview
- **Total Points:** 5.0
- **Required Features:** 9 features

---

## Progress Tracker

### Phase 1: Foundation (1.0 điểm)
- [/] **1. Dependency Injection** (0.5đ)
  - [ ] Create Service interfaces
  - [ ] Create Service implementations  
  - [ ] Register services in DI container
  - [ ] Refactor Pages to use services
  - [ ] Commit #1: `feat: add service layer with DI`
  - [ ] Commit #2: `update: refactor pages to use injected services`

- [ ] **2. MVVM Architecture** (0.5đ)
  - [ ] Create BaseViewModel
  - [ ] Create ViewModels for each page
  - [ ] Update XAML bindings to use ViewModels
  - [ ] Commit #1: `feat: implement MVVM architecture`
  - [ ] Commit #2: `update: complete ViewModel bindings`

---

### Phase 2: Core Features (2.5 điểm)
- [ ] **3. Customer Management** (0.5đ)
  - [ ] Create Customer model
  - [ ] Update Order to reference Customer
  - [ ] Create CustomersPage UI
  - [ ] Implement CRUD for customers
  - [ ] Commit #1: `feat: add Customer model and service`
  - [ ] Commit #2: `update: complete CustomersPage with CRUD`

- [ ] **4. Promotion/Discount** (1.0đ)
  - [ ] Create Promotion model
  - [ ] Create PromotionService
  - [ ] Update Order model for discounts
  - [ ] Create PromotionsPage UI
  - [ ] Integrate promotion in order creation
  - [ ] Commit #1: `feat: add Promotion model and discount types`
  - [ ] Commit #2: `update: integrate promotions into order flow`

- [ ] **5. Advanced Search** (1.0đ)
  - [ ] Create search criteria models
  - [ ] Implement advanced search for products
  - [ ] Implement advanced search for orders
  - [ ] Create advanced search UI panel
  - [ ] Commit #1: `feat: implement advanced search criteria`
  - [ ] Commit #2: `update: add advanced search UI components`

---

### Phase 3: Utilities (1.0 điểm)
- [ ] **6. Print Order (PDF/XPS)** (0.5đ)
  - [ ] Create print template
  - [ ] Implement PDF export
  - [ ] Add print button to order details
  - [ ] Commit #1: `feat: add order print functionality`
  - [ ] Commit #2: `update: complete PDF/XPS export`

- [ ] **7. Backup/Restore Database** (0.25đ)
  - [ ] Implement backup service
  - [ ] Implement restore service
  - [ ] Add UI in ConfigPage
  - [ ] Commit #1: `feat: add database backup/restore`
  - [ ] Commit #2: `update: complete backup UI in settings`

- [ ] **8. Auto Save** (0.25đ)
  - [ ] Implement auto-save service
  - [ ] Add auto-save to order creation
  - [ ] Add auto-save to product creation
  - [ ] Commit #1: `feat: implement auto-save for forms`
  - [ ] Commit #2: `update: add draft recovery on page load`

---

### Phase 4: Quality (0.5 điểm)
- [ ] **9. Test Cases** (0.5đ)
  - [ ] Create test project
  - [ ] Write unit tests for services
  - [ ] Write tests for PasswordHelper
  - [ ] Write tests for promotion calculations
  - [ ] Commit #1: `test: add unit tests for services`
  - [ ] Commit #2: `test: complete test coverage for core features`

---

## Summary

| Feature | Points | Status |
|---------|--------|--------|
| Dependency Injection | 0.5 | 🔄 In Progress |
| MVVM Architecture | 0.5 | ⏳ Pending |
| Customer Management | 0.5 | ⏳ Pending |
| Promotion/Discount | 1.0 | ⏳ Pending |
| Advanced Search | 1.0 | ⏳ Pending |
| Print Order (PDF/XPS) | 0.5 | ⏳ Pending |
| Test Cases | 0.5 | ⏳ Pending |
| Backup/Restore DB | 0.25 | ⏳ Pending |
| Auto Save | 0.25 | ⏳ Pending |
| **TOTAL** | **5.0** | **0/9 Complete** |

---

## Legend
- [ ] Not started
- [/] In progress
- [x] Completed
- ⏳ Pending
- 🔄 In Progress
- ✅ Complete
