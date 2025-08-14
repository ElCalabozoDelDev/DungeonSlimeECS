## Contexto del Proyecto
Este proyecto es un juego estilo snake donde el jugador controla un slime que se mueve por la pantalla, comiendo murcielagos y creciendo. El objetivo es sobrevivir el mayor tiempo posible sin chocar con los muros o con el propio slime.

## Instrucciones para Copilot
1. **Estructura del Código**: El código está organizado en capas. Asegúrate de seguir esta estructura al agregar nuevas funcionalidades o modificar el código existente.
2. **Nombres de Clases y Métodos**: Utiliza nombres descriptivos y en inglés para las clases y métodos. Por ejemplo, `Slime`, `Bat`, `GameController`, etc.
3. **Comentarios**: Añade comentarios claros y concisos en el código para explicar la lógica detrás de las decisiones importantes. Utiliza comentarios en inglés.
4. **Patrones de Diseño**: Utiliza patrones de diseño apropiados cuando sea necesario. Por ejemplo, el patrón Singleton para el controlador del juego y ECS para la gestión de entidades.

## Estructura del Proyecto
- `DungeonSlime/`: Directorio principal del proyecto.
- `DungeonSlime.Engine/`: Contiene la lógica del juego, incluyendo la gestión de entidades y componentes.
	- `ECS/`: Implementación del patrón Entity-Component-System.
	- `Input/`: Manejo de entradas del usuario.
	- `Models/`: Clases modelos utilizadas en el juego.
	- `Scenes/`: Manejo de escenas del juego.
	- `UI`: Contiene la interfaz de usuario del juego.
- `DungeonSlime.Library/`: Contiene las bibliotecas y utilidades compartidas entre diferentes partes del proyecto.
	- `Audio/`: Manejo de audio del juego.
	- `Geometry/`: Clases y métodos relacionados con la geometría.
	- `Graphics/`: Manejo de gráficos y renderizado.
	- `Input/`: Manejo de entradas del usuario.
	- `Scenes/`: Manejo de escenas del juego.

## Tecnologías Utilizadas
- **Lenguaje**: C#
- **Framework**: MonoGame
- **Patrones de Diseño**: ECS (Entity-Component-System), Singleton
- **Herramientas**: Visual Studio, GitHub
- **Control de Versiones**: Git
- **Librerías**: MonoGame.Framework, MonoGame.Extended, Gum.MonoGame