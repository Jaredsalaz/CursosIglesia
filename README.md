# ✝️ Cursos Católicos — Plataforma de Formación en la Fe

Plataforma web de cursos de formación católica en línea, desarrollada con **Blazor Server (.NET 9)**. Permite a los fieles explorar, inscribirse y avanzar en cursos sobre doctrina, Biblia, sacramentos, oración, y más.

---

## 📸 Características

- **Catálogo de Cursos** — Exploración por categorías (Catecismo, Biblia, Sacramentos, Oración, Moral, Historia, Liturgia, Santos) con filtros de búsqueda y nivel de dificultad.
- **Detalle del Curso** — Vista detallada con temario, instructor, duración, valoraciones y lecciones.
- **Inscripción y Progreso** — Los usuarios pueden inscribirse en cursos gratuitos o de pago y seguir su avance.
- **Mis Cursos** — Panel personal para visualizar cursos activos y completados.
- **Plataforma de Aprendizaje** — Interfaz para consumir lecciones con seguimiento de progreso.
- **Perfil de Usuario** — Gestión de datos personales, métodos de pago, seguridad y preferencias de notificaciones.
- **Testimonios** — Reseñas y valoraciones de la comunidad estudiantil.
- **Página "Nosotros"** — Información sobre la misión, visión y equipo detrás de la plataforma.

---

## 🏗️ Arquitectura

El proyecto sigue el patrón **MVVM (Model-View-ViewModel)** con inyección de dependencias:

```
CursosIglesia/
├── Components/
│   ├── Layout/          # MainLayout (navbar + footer)
│   ├── Pages/           # 8 páginas Razor (Home, Cursos, CursoDetalle, etc.)
│   └── Shared/          # Componentes reutilizables (CourseCard, ProfileDropdown)
├── Models/              # 6 modelos de dominio
│   ├── Course.cs        # Curso con lecciones, categoría, instructor
│   ├── Category.cs      # Categorías de cursos
│   ├── Enrollment.cs    # Inscripciones de usuarios
│   ├── Lesson.cs        # Lecciones individuales
│   ├── Testimonial.cs   # Testimonios de estudiantes
│   └── UserProfile.cs   # Perfil de usuario con pagos y notificaciones
├── Services/
│   ├── Interfaces/      # 5 contratos de servicio
│   └── Implementations/ # 5 implementaciones con datos mock
├── ViewModels/          # 6 ViewModels (Home, Courses, CourseDetail, etc.)
├── wwwroot/             # Assets estáticos (CSS, favicon, Bootstrap)
├── Program.cs           # Punto de entrada y configuración de DI
└── appsettings.json     # Configuración de la aplicación
```

---

## 🛠️ Tecnologías

| Tecnología | Versión | Uso |
|---|---|---|
| .NET | 9.0 | Framework principal |
| Blazor Server | — | UI interactiva con SignalR |
| Bootstrap | 5.x | Sistema de grid y componentes |
| Bootstrap Icons | — | Iconografía |
| C# | 13 | Lenguaje de programación |

---

## 🚀 Cómo ejecutar

### Prerrequisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Instrucciones

```bash
# Clonar el repositorio
git clone https://github.com/Jaredsalaz/CursosIglesia.git
cd CursosIglesia

# Restaurar dependencias
dotnet restore

# Ejecutar en modo desarrollo
dotnet run --project CursosIglesia
```

La aplicación estará disponible en `https://localhost:5001` o `http://localhost:5000`.

---

## 📁 Páginas

| Ruta | Página | Descripción |
|---|---|---|
| `/` | Home | Landing page con hero, categorías, cursos destacados, testimonios |
| `/cursos` | Cursos | Catálogo completo con búsqueda y filtros |
| `/curso/{id}` | Detalle | Información completa del curso con temario |
| `/mis-cursos` | Mis Cursos | Cursos inscritos del usuario |
| `/aprender/{id}` | Aprender | Plataforma de aprendizaje con lecciones |
| `/perfil` | Perfil | Gestión de cuenta y preferencias |
| `/nosotros` | Nosotros | Información institucional |

---

## 📝 Notas

- **Datos mock**: Actualmente el proyecto utiliza datos simulados en los servicios. No requiere base de datos para funcionar.
- **Autenticación**: Simulada con un usuario por defecto ("Gerardo Lopez"). Pendiente de integrar un sistema de autenticación real.
- **Responsive**: Diseño adaptable a dispositivos móviles y de escritorio.

---

## 📄 Licencia

Este proyecto es de uso privado. Todos los derechos reservados.

---

> *Ad Maiorem Dei Gloriam* 🕊️
