# ClientesApp
Prueba Técnica C# – Importación de Clientes (Escritorio) y Web API

Objetivo: Dos proyectos en una misma solución. Lo he hecho así porque ya trabajé en un proyecto de esta característica. En este caso, gestiona de forma muy simple una ficha de clientes. No he elegido base de datos para la simplicidad de la prueba, y simplemente será un fichero guardado automáticamente. 

Aplicación de escritorio: Importa clientes desde CSV o JSON, los muestra en una rejilla editable y los guarda en un fichero local al cerrar.
Api: Una Web API REST que expone operaciones de lectura, alta y borrado sobre clientes guardados en un fichero JSON.

Añadidos a la prueba fuera de requisitos: 
He optado por alternar diferentes herramientas de .NET. 
Uso de excepciones.
ClienteDto.cs aparte de las validaciones propias de .net cómo [Required] [Regex] añadidos atributos personalizado. (me gustan en determinadas ocasiones para no sumarle muchas líneas de código al archivo.)

El Dto aparte mostraría sólo la información , que es lo pedido en la prueba, aparte y por el mismo motivo de darle algo de jugo a la API, guardar de forma interna dos datos sin relevancia como ID y Fecha de modificación. (Simplemente para representar que Cliente.cs podría ser el reflejo en sí de la base de datos, mientras que el dto la información que al final es mostrada.)

Herramientas usadas en el proceso; 
ChatGPT en el afinamiento de prompts sobre mi idea principal.
Claude como agente en la programación y destino final de los prompts ya optimizados.
Yo misma para el refinamiento de funcionalidades que no me gustaban demasiado o decidí cambiar porque me parecía mejor sobre la marcha.

A continuación informo de los principales prompts utilizados, tanto el de encargo principal con más contexto y el técnico más pulido a código;

ESPECIFICACIÓN DEL ENCARGO

Quiero que me hagas una prueba técnica en C# con dos soluciones. Hazla lo más
sencilla posible, que no sea pomposa. Comenta el código lo suficiente para que
yo, dentro de dos años, lo entienda sin esfuerzo, y dame cada clase completa y
en orden, diciéndome en qué carpeta va.

Quiero trabajar con tests desde el principio: escribe primero los tests de
cada parte y después el código que los haga pasar.

1. Los datos del cliente
- Cada cliente tiene DNI, nombre, apellidos, fecha de nacimiento, teléfono
  y email.
- Son obligatorios el DNI, el nombre y los apellidos. Los demás son
  opcionales, pero si se rellenan tienen que estar bien escritos.
- El DNI tiene que ser correcto de verdad, y que valga también para los
  extranjeros (NIE). Se hará como Attribute.
- La fecha va en formato normalizado (dd/MM/yyyy) Se hará como Attribute. El teléfono y el email, con una validación básica.
- Cada registro lleva dos datos internos que se guardan en el fichero pero
  nunca se enseñan: un ID aleatorio de .NET y la fecha y hora (con segundos)
  de la última modificación.
- Las reglas se ponen en el modelo con los validadores de .NET ([Required],
  etc.), y quiero las mismas reglas en la API y en la app de escritorio,
  adaptadas a cada una.

2. Los ficheros
- Se importan en CSV (con cabecera dni,nombre,apellidos,fechaNacimiento,
  telefono,email) o en JSON.
- Los datos se guardan en un fichero JSON en la carpeta data/, que se crea
  si no existe. Sin base de datos.

3. La Web API (ASP.NET Core 8)
- Con capa de controlador y un Program.cs limpio.
- GET /clientes, GET /clientes/{dni} (404 si no existe),
  POST /clientes (400 si los datos son inválidos o el DNI ya existe) y
  DELETE /clientes/{dni} (204 si borra, 404 si no existe).
- La API devuelve un DTO sin el ID ni la fecha de modificación. El resto,
  como está.
- La documentación de la api debe ir profesionalmente definida.

4. La aplicación de escritorio (WinForms, .NET Framework 4.8)
- Importar CSV o JSON, a elegir, con barra de progreso.
- Mostrar los clientes en una rejilla, que se carga sola al abrir la
  aplicación y se guarda sola al cerrarla.
- Las columnas obligatorias llevan un * en la cabecera.
- Poder editar y eliminar desde la rejilla, y que se vea claro: un clic
  selecciona la fila entera, doble clic edita solo ese dato, y la tecla
  Supr borra la fila.
- Los errores nunca bloquean nada (ni guardar, ni importar, ni seguir
  editando). Si un dato está mal, se rodea la celda en rojo (solo el borde,
  sin color de fondo) y, debajo, se escribe en ese mismo rojo el ejemplo del
  dato correcto, por ejemplo: Email: nombre@gmail.com.
- Quiero algo discreto: que los colores guíen al usuario para la facilidad de manejo,
 nada más.

5. Los tests
- Cubrir tanto la API como la aplicación de escritorio, aquí tienes que ser crítico y que tengan sentido: que comprueben el funcionamiento real (validaciones, códigos HTTP,  importación, guardado), no detalles de la implementación. Recuérdame revisarlos personalmente.


PROMPT TÉCNICO
Actúa como desarrollador C# senior. Construye con TDD estricto (ciclo
rojo -> verde -> refactor) dos soluciones independientes para gestionar
clientes sin base de datos. Para CADA componente: 1) escribe primero los tests
xUnit que definen su comportamiento y confirma que fallan; 2) implementa el
código mínimo para que pasen; 3) refactoriza sin romper los tests. No avances
al siguiente componente con tests en rojo.

REGLAS GENERALES
- Código lo más simple posible, nada pomposo, sin dependencias innecesarias.
- Comentarios en todo el código, (qué hace cada clase y por qué).
- Entrega la raiz de cada clase completa y en orden, indicando su carpeta.
- Usa validación con DataAnnotations en ambas apps; los validadores propios
  extienden ValidationAttribute y la lógica pura va en un método estático
  testeable.

MODELO DE DATOS
- Campos: dni, nombre, apellidos, fechaNacimiento, telefono, email.
- Obligatorios: dni, nombre, apellidos ([Required]). Opcionales: fecha,
  teléfono, email (si vienen vacíos no se validan; si vienen rellenos, deben
  tener formato correcto).
- Reglas: DNI/NIE correcto (letra = TRWAGMYFPDXBNJZSQVHLCKE[número % 23];
  NIE: X=0, Y=1, Z=2), fecha dd/MM/yyyy real, teléfono ^\+?[0-9 ]{9,15}$,
  email ^[^@\s]+@[^@\s]+\.[^@\s]+$.
- Datos internos por registro, guardados en el JSON y nunca mostrados:
  Id (Guid.NewGuid()) y FechaModificacion (DateTime con hora y segundos).
- CSV con cabecera dni,nombre,apellidos,fechaNacimiento,telefono,email.
  JSON: array de objetos en camelCase. Fichero: data/clientes_store.json
  junto al ejecutable (crear la carpeta si no existe).

SOLUCIÓN 1: WEB API (ASP.NET Core 8, controladores)
- Capas: Controllers, Models (entidad Cliente + ClienteDto), Services
  (IClienteStore + ClienteStore en fichero JSON con lock), Validation.
- La API recibe y devuelve ClienteDto (sin Id ni FechaModificacion); las
  reglas de validación van en el DTO y [ApiController] devuelve el 400.
- Endpoints: GET /clientes (200), GET /clientes/{dni} (200/404),
  POST /clientes (201/400 si inválido o DNI repetido),
  DELETE /clientes/{dni} (204/404). DNI comparado sin distinguir mayúsculas.
- Program.cs limpio: AddControllers, Swagger solo en desarrollo, store
  singleton, `public partial class Program { }` para los tests.
- ClienteStore con constructor con ruta (tests con fichero temporal).
- Tests: validadores, DTO (reglas, conversión, y que NO expone Id ni
  FechaModificacion), store con fichero temporal, controlador con store
  falso, y la API completa con WebApplicationFactory (códigos HTTP,
  validaciones, datos internos ocultos, persistencia del Id y la fecha).


SOLUCIÓN 2: ESCRITORIO (WinForms, .NET Framework 4.8)
- Capas: Models (ClienteDesktop), Validation, Services (ValidadorCliente,
  ImportadorClientes, AlmacenClientes), FormPrincipal por código.
- Mismas reglas que la API con DataAnnotations; en ErrorMessage va el EJEMPLO
  correcto (p. ej. nombre@gmail.com), no un mensaje de error.
- Importar CSV/JSON a elegir con barra de progreso, en segundo plano. La
  importación NO rechaza nada: todo entra y lo incorrecto se marca en rojo.
  Solo se ignoran DNIs repetidos (se informa en un resumen).
- Rejilla (DataGridView + BindingList): columnas definidas a mano sin Id ni
  FechaModificacion; las obligatorias con * en la cabecera (deducido del
  [Required]); un clic selecciona la fila entera; doble clic edita solo ese
  dato; tecla Supr y botón Eliminar borran filas con confirmación;
  fila vacía para añadir; FechaModificacion se actualiza al editar.
- Avisos que nunca bloquean: celda con error rodeada de rojo (solo borde,
  sin fondo) y, bajo la rejilla, el ejemplo correcto en el mismo rojo
  ("Email: nombre@gmail.com"). Línea de ayuda gris con las acciones.
- Persistencia: cargar el fichero al abrir y guardar todo al cerrar, aunque
  haya errores. JSON en camelCase compartido con la API.
- AlmacenClientes con constructor con ruta (tests con fichero temporal).
- Tests: validadores, cliente por defecto, ValidadorCliente, importador
  (CSV/JSON con ficheros reales, datos malos que se importan, progreso),
  almacén. FormPrincipal no se prueba: su lógica vive en clases testeables.



PROMPTS DE AFINAMIENTO: 
- Avisos: celda con error rodeada de rojo (solo borde, sin fondo); si está
  vacía, el ejemplo correcto dentro en rojo tenue; y bajo la rejilla el
  ejemplo en el mismo rojo ("Email: nombre@gmail.com"). Los errores no bloquean
  editar ni importar.
- Persistencia: cargar el fichero al abrir. Al cerrar, guardar solo los
  clientes con los obligatorios correctos; si hay alguno que no, avisar de que
  no se guardará (Aceptar descarta y cierra, Cancelar vuelve a corregir).
  Si el JSON (al cargar o al importar) tiene formato incorrecto, lanzar
  FormatException y mostrar "Formato incorrecto". JSON en camelCase compartido
  con la API.
- Los errores no bloquean editar ni importar. Si un dato está mal, se rodea la
  celda en rojo (solo el borde, sin fondo); si está vacía, se escribe dentro el
  ejemplo correcto en rojo tenue, y debajo, en ese mismo rojo, el ejemplo del
  dato correcto, por ejemplo: Email: nombre@gmail.com.
- Al cerrar solo se guardan los clientes con los datos obligatorios bien
  rellenados. Si hay alguno que no, se avisa de que no se guardará y se deja
  elegir entre descartar y cerrar, o volver a corregirlo.
- Si un fichero JSON tiene un formato incorrecto, se avisa con un mensaje claro.



