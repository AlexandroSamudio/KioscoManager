[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Angular](https://img.shields.io/badge/Angular-17+-red)](https://angular.io)
[![.NET](https://img.shields.io/badge/.NET-8-purple)](https://dotnet.microsoft.com)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-13+-blue)](https://postgresql.org)
<h1 align="left">Kiosco Manager</h1>
<br />
<div align="left">
  <a href="https://github.com/github_username/repo_name">
    <img width="535" height="151" alt="Screenshot from 2025-08-15 20-34-46" src="https://github.com/user-attachments/assets/81574e51-6a2d-4f76-b9d7-dd894b0f4a78" />
  </a>

  <p align="left">
    Aplicación web diseñada específicamente para kioscos y pequeñas tiendas de conveniencia, que funciona como un sistema de punto de venta (POS) y gestión de inventario.
    <br />
    <a href="https://github.com/AlexandroSamudio/KioscoManager">Link al sitio</a>
    &middot;
    <a href="https://github.com/AlexandroSamudio/KioscoManager/issue">Reportar Bug</a>
    &middot;
    <a href="https://github.com/AlexandroSamudio/KioscoManager/discussions">Solicitar Feature</a>
  </p>
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Tabla de contenidos</summary>
  <ol>
    <li>
      <a href="#about-the-project">Acerca del proyecto</a>
      <ul>
        <li><a href="#built-with">Desarrollado con</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Instalación y configuración</a>
    </li>
    <li><a href="#usage">Guía de uso</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contribuir</a></li>
    <li><a href="#license">Licencia</a></li>
    <li><a href="#contact">Contacto y comunidad</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## ❓ Acerca del proyecto
### 🎥 Demostración en Video
El siguiente video muestra el flujo completo de la aplicación en un minuto:

https://github.com/user-attachments/assets/6515e98a-840e-4739-aa37-c9fd7d9ccb10

KioscoManager nace de la observación directa de las necesidades reales de pequeños comercios. Muchos kioscos y tiendas de conveniencia siguen dependiendo de métodos manuales o sistemas anticuados que limitan su potencial de crecimiento.



### 🚀 Desarrollado con

![Angular](https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-007ACC?style=for-the-badge&logo=typescript&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![Nginx](https://img.shields.io/badge/Nginx-009639?style=for-the-badge&logo=nginx&logoColor=white)



<!-- GETTING STARTED -->
## 🛠️ Instalación y configuración

1. Clonar el repositorio
   ```sh
   git clone https://github.com/AlexandroSamudio/KioscoManager
   cd kiosco-manager
   ```
2. Configurar el backend
   ```sh
   cd backend
   dotnet restore
   dotnet ef database update
   dotnet run
   ```
3. Configurar el frontend
   ```js
   cd frontend
   npm install
   ng serve
   ```
4. Variables de entorno
   ```sh
   CONNECTION_STRING=Server=localhost;Database=KioscoManagerDB;User Id=postgres;Password=tu_password;
   ```



<!-- USAGE EXAMPLES -->
## 📋 Guía de Uso

### 🏪 Para Administradores (Dueños de Kiosco)

#### Primer Uso
1. **Registro**: Crea tu cuenta con email y contraseña
2. **Setup Inicial**: Configura los datos de tu kiosco
3. **Cargar Productos**: Añade tu inventario con códigos de barras
4. **Invitar Empleados**: Genera códigos de invitación seguros

#### Operación Diaria
- **Dashboard**: Monitorea ventas y alertas de stock
- **Reportes**: Analiza rentabilidad y productos más vendidos
- **Inventario**: Actualiza precios y cantidades

### 👥 Para Empleados

#### Ingreso al Sistema
1. Solicita código de invitación al administrador
2. Regístrate usando el código
3. Accede al dashboard de tu kiosco

#### Proceso de Venta
1. **Escanear productos** o buscar por nombre
2. **Verificar cantidades** en el ticket digital
3. **Procesar pago** y finalizar venta
4. El **stock se actualiza automáticamente**


<!-- ROADMAP -->
## 🗺️ Roadmap
### ✅ MVP (Completado)
- [x] Sistema de autenticación JWT
- [x] Gestión completa de productos (CRUD)
- [x] Sistema de punto de venta
- [x] Dashboard básico con KPIs
- [x] Sistema de roles (Admin/Empleado)
### 🔄 En desarrollo
- [ ] Identificación de foto para cada producto
- [ ] Intregración de Cloudinary para las fotos



<!-- CONTRIBUTING -->
## 🤝 Contribuir

¡Las contribuciones son bienvenidas! Por favor:

1. Haz fork del repositorio
2. Crea una rama para tu feature (`git checkout -b feat/nueva-funcionalidad`)
3. Haz commit de tus cambios (`git commit -m 'Añadir nueva funcionalidad'`)
4. Push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Abre un Pull Request



<!-- LICENSE -->
## Licencia

Este proyecto está bajo la Licencia MIT. Ver el archivo [LICENSE](LICENSE) para más detalles.



<!-- CONTACTO -->
## Contacto y Comunidad
- 📧 **Email**: alexandrosamudio27@gmail.com
- 💼 **LinkedIn**: https://www.linkedin.com/in/alexandro-samudio-b40b76289/
