## Contexto del Proyecto
Este proyecto es un juego estilo snake donde el jugador controla un slime que se mueve por la pantalla, comiendo murcielagos y creciendo. El objetivo es sobrevivir el mayor tiempo posible sin chocar con los muros o con el propio slime.

## Instrucciones para Copilot
1. **Estructura del C�digo**: El c�digo est� organizado en capas. Aseg�rate de seguir esta estructura al agregar nuevas funcionalidades o modificar el c�digo existente.
2. **Nombres de Clases y M�todos**: Utiliza nombres descriptivos y en ingl�s para las clases y m�todos. Por ejemplo, `Slime`, `Bat`, `GameController`, etc.
3. **Comentarios**: A�ade comentarios claros y concisos en el c�digo para explicar la l�gica detr�s de las decisiones importantes. Utiliza comentarios en ingl�s.
4. **Patrones de Dise�o**: Utiliza patrones de dise�o apropiados cuando sea necesario. Por ejemplo, el patr�n Singleton para el controlador del juego y ECS para la gesti�n de entidades.

## Estructura del Proyecto
- `DungeonSlime/`: Directorio principal del proyecto.
- `DungeonSlime.Engine/`: Contiene la l�gica del juego, incluyendo la gesti�n de entidades y componentes.
	- `ECS/`: Implementaci�n del patr�n Entity-Component-System.
	- `Input/`: Manejo de entradas del usuario.
	- `Models/`: Clases modelos utilizadas en el juego.
	- `Scenes/`: Manejo de escenas del juego.
	- `UI`: Contiene la interfaz de usuario del juego.
- `DungeonSlime.Library/`: Contiene las bibliotecas y utilidades compartidas entre diferentes partes del proyecto.
	- `Audio/`: Manejo de audio del juego.
	- `Geometry/`: Clases y m�todos relacionados con la geometr�a.
	- `Graphics/`: Manejo de gr�ficos y renderizado.
	- `Input/`: Manejo de entradas del usuario.
	- `Scenes/`: Manejo de escenas del juego.

## Tecnolog�as Utilizadas
- **Lenguaje**: C#
- **Framework**: MonoGame
- **Patrones de Dise�o**: ECS (Entity-Component-System), Singleton
- **Herramientas**: Visual Studio, GitHub
- **Control de Versiones**: Git
- **Librer�as**: MonoGame.Framework, MonoGame.Extended, Gum.MonoGame