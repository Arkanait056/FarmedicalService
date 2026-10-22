# FarmedicalService

Sistema de punto de venta para farmacia (Windows Forms, C#, SQLite).

- Facturación con ITBIS, control de cambio e impresión térmica de recibos
- Inventario con lotes, fechas de vencimiento y rotación FEFO
- Roles: Administrador, Supervisor, Cajero
- Reportes de ventas diarias y mensuales

## Requisitos
- Windows
- .NET 10 SDK

## Ejecutar
```
dotnet run
```
La base de datos `Farmedical.db` se crea sola en la carpeta de salida al iniciar.

## Interfaz

La interfaz usa una capa de UI propia en `FarmedicalService/UI/`:

- `Theme.cs`: paleta (verde farmacia) y tipografía (Segoe UI).
- `Ui.cs`: escalado por DPI (`Ui.S`), fábricas de campos y estilo de tablas.
- `Controls.cs`: `Card`, `AppButton`, `NavItem`, `KpiCard`, `ModuleCard`, `IconBadge`.
- `Icons.cs`: iconos vectoriales dibujados con GDI+ (sin emojis ni fuentes externas).
- `PageForm.cs` / `DialogForm.cs`: base de las pantallas (dentro de la ventana principal) y de los diálogos.

Reglas para nuevas pantallas: sin `Location = new Point(...)`; usar `TableLayoutPanel`/`FlowLayoutPanel`/`Dock`
y medidas con `Ui.S(...)` para que se vea bien a 100 %, 125 % y 150 % de escala de Windows.

## Datos fiscales y seguridad

- `FarmaciaConfig.cs`: nombre, RNC, dirección, teléfono y serie/secuencial del NCF que se imprimen en el recibo.
  Reemplaza los valores de ejemplo y valida el rango autorizado por la DGII antes de emitir comprobantes reales.
- Las contraseñas se guardan con PBKDF2 (las antiguas en texto plano se convierten al iniciar sesión).
- Las bases de datos existentes se actualizan solas (columnas nuevas para anulaciones, método de pago, NCF y cuadre de caja).
