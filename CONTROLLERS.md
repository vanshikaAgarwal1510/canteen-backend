# Canteen Automation API Controllers

This document is the consolidated reference for the controllers in
`CanteenAutomationApi/Controllers`.

## Conventions

- Unless noted otherwise, requests use `POST`.
- `JSON` means the request is sent as JSON in the request body.
- `Form` means `multipart/form-data` and is bound with `[FromForm]`.
- `Authenticated` means a valid authenticated user is required.
- Role names are case-sensitive: `Admin`, `Staff`, and `User`.
- The response body and status code are determined by the implementation in
  the linked controller.

## Authentication and public controllers

### LoginController

Source: [LoginController.cs](CanteenAutomationApi/Controllers/LoginController.cs)

Base route: `/api/login`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/login/admin` | Public | `LoginRequest` (JSON) |
| POST | `/api/login/send-otp` | Public | `SendOtpRequest` (JSON) |
| POST | `/api/login/verify-otp` | Public | `VerifyOtpRequest` (JSON) |

### LogoutController

Source: [LogoutController.cs](CanteenAutomationApi/Controllers/LogoutController.cs)

Base route: `/api/logout`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/logout` | Public | `LogoutRequest` (JSON) |

### SignupController

Source: [SignupController.cs](CanteenAutomationApi/Controllers/SignupController.cs)

Base route: `/api/signup`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/signup` | Public | `SignupRequest` (JSON) |

### MenuController

Source: [GetMenuController.cs](CanteenAutomationApi/Controllers/GetMenuController.cs)

Base route: `/api/menu`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/menu` | Authenticated | `GetMenuRequest` (JSON) |

## User controllers

### InitController

Source: [InitController.cs](CanteenAutomationApi/Controllers/User/InitController.cs)

Base route: `/api/init`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/init` | Public | `InitDataRequest` (JSON) |
| GET | `/api/init/check-token` | Authenticated | None |
| POST | `/api/init/get-payment-method` | As configured by the action | `GetPaymentMethodRequest` (JSON) |

### GetMyOrderController

Source: [GetMyOrderController.cs](CanteenAutomationApi/Controllers/User/GetMyOrderController.cs)

Base route: `/api/my-order`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/my-order` | `User` role | `OrderRequest` (JSON) |
| POST | `/api/my-order/details` | `User` role | `OrderDetailsRequest` (JSON) |

### PlaceOrderController

Source: [PlaceOrderController.cs](CanteenAutomationApi/Controllers/User/PlaceOrderController.cs)

Base route: `/api/`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/place-order` | `User` role | `PlaceOrderRequest` (JSON) |
| POST | `/api/check-order` | `User` role | `CheckOrderRequest` (JSON) |

### CancelOrderController

Source: [CancelOrderController.cs](CanteenAutomationApi/Controllers/User/CancelOrderController.cs)

Base route: `/api/cancel-order`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/cancel-order` | `User` role | Request type defined by the action signature (JSON) |

### ProfileController

Source: [ProfileController.cs](CanteenAutomationApi/Controllers/User/ProfileController.cs)

Base route: `/api/profile`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/profile` | `User` role | `ProfileRequest` (JSON) |

### RatingsController

Source: [RatingController.cs](CanteenAutomationApi/Controllers/User/RatingController.cs)

Base route: `/api/Rating` (resolved from `[controller]`)

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/Rating` | `User` role | `AddRatingRequest` (JSON) |

### VadilateCouponController

Source: [ValidateCouponController.cs](CanteenAutomationApi/Controllers/User/ValidateCouponController.cs)

Base route: `/api/coupons`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/coupons` | `User` role | `ValidateCouponRequest` (JSON) |

### WalletController

Source: [WalletTransactionController.cs](CanteenAutomationApi/Controllers/User/WalletTransactionController.cs)

Base route: `/api/wallet`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/wallet/history` | `User` role | `GetWalletHistoryRequest` (JSON) |

## Admin and staff controllers

### AdminDashboardController

Source: [AdminDashboardController.cs](CanteenAutomationApi/Controllers/Admin/AdminDashboardController.cs)

Base route: `/api/admin-dashboard`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/admin-dashboard` | `Admin` role | `DashboardDataRequest` (JSON) |

### CouponController

Source: [CouponController.cs](CanteenAutomationApi/Controllers/Admin/CouponController.cs)

Base route: `/api/coupon`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/coupon/get-coupon` | `Admin` role | `GetCouponRequest` (JSON) |
| POST | `/api/coupon/add-coupon` | `Admin` role | `AddCouponRequest` (JSON) |
| POST | `/api/coupon/update-coupon` | `Admin` role | `UpdateCouponRequest` (JSON) |
| POST | `/api/coupon/delete-coupon` | `Admin` role | `DeleteCouponRequest` (JSON) |

### FaqController

Source: [FaqController.cs](CanteenAutomationApi/Controllers/Admin/FaqController.cs)

Base route: `/api/faq`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/faq/get-faqList` | `Admin` or `User` role | `GetFaqRequest` (JSON) |
| POST | `/api/faq/add-faq` | `Admin` role | `AddEditFaqRequest` (JSON) |
| POST | `/api/faq/delete-faq` | `Admin` role | `DeleteFaqRequest` (JSON) |

### GetOrdersController

Source: [GetAllOrdersController.cs](CanteenAutomationApi/Controllers/Admin/GetAllOrdersController.cs)

Base route: `/api/get-orders`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/get-orders/all` | `Admin` role | `FilteredOrdersRequest` (JSON) |
| POST | `/api/get-orders/active` | `Staff` role | `GetMenuRequest` (JSON) |
| POST | `/api/get-orders/details` | `Admin` or `Staff` role | `OrderDetailsRequestDto` (JSON) |

### Admin InitController

Source: [InitController.cs](CanteenAutomationApi/Controllers/Admin/InitController.cs)

Base route: `/api/auth`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/auth` | As configured by the action | `InitRequest` (JSON) |

### InvoiceController

Source: [InvoiceController.cs](CanteenAutomationApi/Controllers/Admin/InvoiceController.cs)

Base route: `/api/invoice`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/invoice/get-invoice` | Authenticated | `InvoiceRequestDto` (JSON) |

### MenuCategoryController

Source: [MenuCategoryController.cs](CanteenAutomationApi/Controllers/Admin/MenuCategoryController.cs)

Base route: `/api/MenuCategory` (resolved from `[controller]`)

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/MenuCategory/add-menu-category` | `Admin` role | `AddMenuCategoryRequest` (Form) |
| POST | `/api/MenuCategory/update-menu-category` | `Admin` role | `UpdateCategoryRequest` (Form) |
| POST | `/api/MenuCategory/delete-menu-category` | `Admin` role | `DeleteCategoryRequest` (JSON) |

### MenuItemController

Source: [MenuItemController.cs](CanteenAutomationApi/Controllers/Admin/MenuItemController.cs)

Base route: `/api/MenuItem` (resolved from `[controller]`)

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/MenuItem/add-menu-item` | `Admin` role | `AddMenuItemRequest` (Form) |
| POST | `/api/MenuItem/update-menu-item` | `Admin` role | `UpdateItemRequest` (Form) |
| POST | `/api/MenuItem/delete-menu-item` | `Admin` role | `DeleteItemRequest` (JSON) |

### PaymentMethodController

Source: [PaymentMethodController.cs](CanteenAutomationApi/Controllers/Admin/PaymentMethodController.cs)

Base route: `/api/payment-method`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/payment-method/get-payment-method` | `Admin` role | `GetPaymentMethodRequest` (JSON) |
| POST | `/api/payment-method/add-payment-method` | `Admin` role | `AddPaymentMethodRequest` (JSON) |
| POST | `/api/payment-method/update-payment-method` | `Admin` role | `UpdatePaymentMethodRequest` (JSON) |
| POST | `/api/payment-method/delete-payment-method` | `Admin` role | `DeletePaymentMethodRequest` (JSON) |

### SettingsController

Source: [SettingsController.cs](CanteenAutomationApi/Controllers/Admin/SettingsController.cs)

Base route: `/api/settings`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/settings/details` | `Admin` role | `GetStaffRequest` (JSON) |
| POST | `/api/settings/profile` | `Admin` role | `AdminProfileRequest` (Form) |
| POST | `/api/settings/change-password` | `Admin` role | `ChangePasswordRequest` (JSON) |
| POST | `/api/settings/update-canteen-detail` | `Admin` role | `CanteenDetailRequest` (JSON) |
| POST | `/api/settings/update-order-detail` | `Admin` role | `CanteenOrderRequest` (JSON) |

### StaffController

Source: [StaffController.cs](CanteenAutomationApi/Controllers/Admin/StaffController.cs)

Base route: `/api/staff`

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/staff/get-staff` | `Admin` role | `GetStaffRequest` (JSON) |
| POST | `/api/staff/add-staff` | `Admin` role | `AddStaffRequest` (Form) |
| POST | `/api/staff/update-staff` | `Admin` role | `UpdateStaffRequest` (JSON) |
| POST | `/api/staff/delete-staff` | `Admin` role | `DeleteStaffRequest` (JSON) |

### OrdersController

Source: [UpdateOrderController.cs](CanteenAutomationApi/Controllers/Admin/UpdateOrderController.cs)

Base route: `/api/Orders` (resolved from the `OrdersController` class name)

| Method | Endpoint | Access | Request |
|---|---|---|---|
| POST | `/api/Orders/mark-payment-paid` | `Admin` or `Staff` role | `UpdatePaymentStatusRequest` (JSON) |
| POST | `/api/Orders/update-status` | `Admin` or `Staff` role | `UpdateOrderRequest` (JSON) |
| POST | `/api/Orders/validate-pickup-code` | `Admin` or `Staff` role | `ValidatePickupCodeRequest` (JSON) |

## Source inventory

There are 24 controllers in total:

- Root: `LoginController`, `LogoutController`, `SignupController`, `MenuController`
- User: `InitController`, `GetMyOrderController`, `PlaceOrderController`,
  `CancelOrderController`, `ProfileController`, `RatingsController`,
  `VadilateCouponController`, `WalletController`
- Admin: `AdminDashboardController`, `CouponController`, `FaqController`,
  `GetOrdersController`, `InitController`, `InvoiceController`,
  `MenuCategoryController`, `MenuItemController`, `PaymentMethodController`,
  `SettingsController`, `StaffController`, `OrdersController`

