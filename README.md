# Generador de Casos de Prueba - Gramáticas Libres de Contexto

## Descripción del Proyecto

Aplicación de escritorio desarrollada en **C# con Avalonia UI** capaz de generar automáticamente casos de prueba (válidos, inválidos y extremos) a partir de una **Gramática Libre de Contexto (GLC)**.

El sistema implementa derivación formal, mutación sintáctica y generación de casos extremos para testing exhaustivo de parsers, compiladores y validadores.

---

## Funcionalidades Implementadas

### 1. Generación de Casos Válidos
- Derivación leftmost (por la izquierda) desde el símbolo inicial
- Algoritmo inteligente que evita recursión infinita
- Control de profundidad máxima
- Historial paso a paso de la derivación

### 2. Generación de Casos Inválidos (Mutación Sintáctica)
- **Paréntesis desbalanceados**: Elimina o agrega paréntesis
- **Operadores duplicados**: `id + + id`
- **Operador al inicio**: `+ id * id`
- **Operador al final**: `id + id *`
- **Paréntesis vacío**: `( )`
- **Operador faltante**: `id id` (dos operandos consecutivos)
- **Identificador faltante**: `id + * id`
- **Carácter inválido**: `id @ id`
- **Espacios en medio de token**: `i d + id`

### 3. Generación de Casos Extremos
- **Profundidad máxima**: Derivación muy profunda
- **Profundidad mínima**: Caso más simple posible
- **Complejidad máxima**: Máximo número de operadores
- **Expresión larga**: Muchos símbolos terminales
- **Expresión corta**: Mínima expresión válida
- **Anidamiento máximo**: Múltiples niveles de paréntesis

### 4. Sistema de Clasificación Automática
- Categoriza cada caso: válido/inválido/extremo
- Metadata completa: profundidad, operadores, longitud, etc.
- IDs únicos para cada caso

### 5. Exportación a JSON
- Formato estructurado con toda la información
- Incluye gramática utilizada
- Métricas completas del proceso
- Configuración de generación
- Timestamp y versión
- **Ubicación**: Se guarda en la carpeta del proyecto con nombre `casos_prueba_YYYYMMDD_HHMMSS.json`

### 6. Sistema de Métricas
- **Distribución porcentual** por categoría
- **Longitud promedio** de expresiones
- **Profundidad máxima** del árbol sintáctico
- **Operadores generados** por tipo (+, *, -, /)
- **Tipos de mutación** aplicados
- **Tipos de casos extremos** generados
- **Tiempo de ejecución**

### 7. Parser de Gramáticas desde TXT
- Carga gramáticas desde archivos de texto
- Formato simple: `E -> E + T`
- Soporta comentarios con `#`
- Detección automática de terminales/no terminales
- **Ubicación esperada**: `gramatica_ejemplo.txt` en la carpeta del proyecto

---

## Arquitectura del Sistema

```
Gramática G (TXT) --> ParserGramatica
                            |
                            v
                   ContextFreeGrammar
                            |
              +-------------+-------------+
              |             |             |
              v             v             v
    GeneradorDerivaciones  Mutador  GeneradorCasosExtremos
    (Casos válidos)    (Inválidos)   (Extremos)
              |             |             |
              +-------------+-------------+
                            v
                     Clasificador
                            |
                            v
                    Lista de CasoPrueba
                            |
              +-------------+-------------+
              |             |             |
              v             v             v
       GeneradorMetricas  ExportadorJSON  UI
```

---

## Casos de Uso

### Uso 1: Generar 1 Caso Válido con Historial
1. Ajusta la **profundidad máxima** (default: 20)
2. Click en **"Caso Válido + Historial"** (botón verde)
3. Ver derivación paso a paso en panel derecho

**Resultado:**
```
Paso 0: E
Paso 1: E + T (aplicando: E → E + T)
Paso 2: T + T (aplicando: E → T)
Paso 3: F + T (aplicando: T → F)
Paso 4: id + T (aplicando: F → id)
Paso 5: id + F (aplicando: T → F)
Paso 6: id + id (aplicando: F → id)
```

### Uso 2: Generar Múltiples Casos Válidos
1. Configura **cantidad de casos válidos** (slider verde)
2. Click en **"Casos Válidos"** (botón azul)
3. Ver lista de expresiones generadas

### Uso 3: Generar Casos Inválidos
1. Configura **cantidad de casos inválidos** (slider rojo)
2. Click en **"Casos Inválidos"** (botón rojo)
3. Ver mutaciones con tipo entre corchetes:
   ```
   [OperadorDuplicado] id + + id
   [ParentesisDesbalanceados] ( id + id
   [CaracterInvalido] id @ id
   ```

### Uso 4: Generar Casos Extremos
1. Click en **"Casos Extremos"** (botón naranja)
2. Ver casos extremos generados con sus tipos

### Uso 5: Generar Suite Completa
1. Configura cantidades deseadas en los sliders
2. Click en **"Generar Todo"** (botón turquesa grande)
3. Sistema genera:
   - N casos válidos
   - M casos inválidos
   - K casos extremos (automático)
4. Ver **Reporte de Métricas** completo en panel derecho

**Ejemplo de Métricas:**
```
+===============================================================+
|          REPORTE DE MÉTRICAS - GENERACIÓN DE CASOS           |
+===============================================================+

[RESUMEN GENERAL]
-----------------------------------------------------------------
  Total de casos generados: 21
  Tiempo de ejecución: 245 ms
  Tiempo promedio por caso: 11.67 ms

[DISTRIBUCIÓN POR CATEGORÍA]
-----------------------------------------------------------------
  Válidos:        5 ( 23.8%)
  Inválidos:      5 ( 23.8%)
  Extremos:      11 ( 52.4%)

[ESTADÍSTICAS DE LONGITUD (tokens)]
-----------------------------------------------------------------
  Promedio: 7.52
  Mínima:   1
  Máxima:   15

[OPERADORES GENERADOS]
-----------------------------------------------------------------
  +   :    12 ( 48.0%)
  *   :    10 ( 40.0%)
  -   :     3 ( 12.0%)
  Total: 25
```

### Uso 6: Exportar a JSON
1. Después de generar suite completa
2. Click en **"Exportar JSON"** (botón morado)
3. **Archivo se guarda automáticamente en la carpeta del proyecto**
4. Nombre: `casos_prueba_YYYYMMDD_HHMMSS.json`
5. Mensaje de confirmación muestra la ubicación

**Estructura del JSON:**
```json
{
  "gramatica": {
    "nombre": "Gramática de Expresiones Aritméticas",
    "simboloInicial": "E",
    "variables": ["E", "T", "F"],
    "terminales": ["+", "*", "(", ")", "id"],
    "producciones": ["E → E + T", "E → T", ...]
  },
  "casos": [
    {
      "id": "VALID_0001",
      "cadena": "id + id * id",
      "categoria": "Valido",
      "descripcion": "Caso válido generado por derivación",
      "metadata": {
        "profundidad": 8,
        "num_tokens": 5,
        "total_operadores": 2,
        ...
      }
    }
  ],
  "metricas": {
    "totalCasos": 21,
    "casosValidos": 5,
    "distribucionPorcentual": {...},
    ...
  }
}
```

### Uso 7: Cargar Gramática desde Archivo
1. Crea un archivo `gramatica_ejemplo.txt` en la **carpeta del proyecto**
2. Click en **"Cargar desde archivo (.txt)"** (botón morado superior)
3. Sistema carga y muestra la información de la gramática

---

## Cómo Ejecutar

### Requisitos:
- .NET SDK 8.0 o superior
- Windows (aplicación de escritorio)

### Pasos:
```powershell
# 1. Clonar/Descargar el proyecto
cd miniproyecto2_info1148

# 2. Restaurar dependencias
dotnet restore

# 3. Compilar
dotnet build

# 4. Ejecutar
dotnet run
```

---

## Estructura del Proyecto

```
miniproyecto2_info1148/
├── Models/                          # Lógica de negocio
│   ├── ContextFreeGrammar.cs       # Definición formal de GLC
│   ├── GeneradorDerivaciones.cs    # Derivación leftmost
│   ├── Mutador.cs                  # Generación de casos inválidos
│   ├── GeneradorCasosExtremos.cs   # Generación de edge cases
│   ├── Clasificador.cs             # Clasificación automática
│   ├── ExportadorJSON.cs           # Exportación a JSON
│   ├── GeneradorMetricas.cs        # Cálculo de estadísticas
│   ├── ParserGramatica.cs          # Parser de archivos TXT
│   ├── Symbol.cs                   # Clase base de símbolos
│   ├── Terminal.cs                 # Símbolos terminales
│   ├── NonTerminal.cs              # Símbolos no terminales
│   ├── Production.cs               # Reglas de producción
│   └── GramaticaExpresionesAritmeticas.cs  # Gramática ejemplo
├── ViewModels/                      # Lógica de presentación
│   └── MainWindowViewModel.cs
├── Views/                           # Interfaz gráfica
│   └── MainWindow.axaml
└── README.md                        # Este archivo
```

---

## Interfaz de Usuario

### Panel Izquierdo - Configuración:
- **Botón Morado Superior**: Cargar gramática desde archivo TXT
- **Slider Profundidad** (5-100): Control de profundidad máxima de derivación
- **Slider Casos Válidos** (1-20): Cantidad de casos válidos a generar
- **Slider Casos Inválidos** (1-20): Cantidad de casos inválidos a generar

### Acciones Rápidas:
- **Botón Verde**: Genera 1 caso válido con historial detallado
- **Botón Azul**: Genera múltiples casos válidos
- **Botón Rojo**: Genera casos inválidos (mutación)
- **Botón Naranja**: Genera casos extremos

### Suite Completa:
- **Botón Turquesa Grande**: Genera suite completa (válidos + inválidos + extremos)
- **Botón Morado**: Exporta todo a JSON en la carpeta del proyecto
- **Botón Gris**: Limpia todos los resultados

### Panel Derecho - Resultados:
- Resumen de generación
- Lista de casos generados
- Historial de derivación (modo detallado)
- Reporte de métricas (suite completa)

---

## Métricas Calculadas

1. **Cantidad total** de casos generados
2. **Distribución porcentual** (válidos/inválidos/extremos)
3. **Longitud promedio** de expresiones (número de tokens)
4. **Profundidad máxima** de derivación
5. **Operadores por tipo** (+, *, -, /)
6. **Niveles de mutación** aplicados con conteo por tipo
7. **Tipos de casos extremos** generados con conteo
8. **Tiempo de ejecución** total y promedio por caso

---

## Formato de Archivo de Gramática TXT

### Ubicación:
El archivo debe llamarse `gramatica_ejemplo.txt` y estar en la **carpeta del proyecto**.

### Formato:
```txt
# Gramática de Expresiones Aritméticas
# Líneas con # son comentarios y se ignoran

E -> E + T
E -> T
T -> T * F
T -> F
F -> ( E )
F -> id
```

### Reglas:
- Una producción por línea
- Formato: `NoTerminal -> Simbolo1 Simbolo2 ...`
- Separador: `->` (guión y mayor que)
- Símbolos separados por espacios
- No terminales: Letras mayúsculas o PascalCase (E, T, F, Expresion)
- Terminales: Todo lo demás (+, *, id, etc.)
- Primera producción define el símbolo inicial
- Líneas vacías se ignoran
- Comentarios con `#` al inicio de línea

### Ejemplo Completo:
```txt
# Gramática para expresiones aritméticas simples
# Soporta suma, multiplicación y paréntesis

# Producciones para expresiones
E -> E + T
E -> T

# Producciones para términos
T -> T * F
T -> F

# Producciones para factores
F -> ( E )
F -> id
F -> numero
```

---

## Archivos de Salida

### Archivo JSON Exportado:
- **Nombre**: `casos_prueba_YYYYMMDD_HHMMSS.json`
- **Ubicación**: Carpeta del proyecto
- **Contenido**:
  - Información completa de la gramática
  - Todos los casos generados con metadata
  - Métricas del proceso
  - Configuración utilizada
  - Timestamp de generación

---

## Tecnologías Utilizadas

- **Lenguaje**: C# 12
- **Framework**: .NET 8.0
- **UI**: Avalonia UI 11.3.8
- **MVVM**: CommunityToolkit.Mvvm 8.2.1
- **JSON**: System.Text.Json
- **Arquitectura**: MVVM (Model-View-ViewModel)

---

## Notas Técnicas

### Derivación Leftmost:
- Expande siempre el no terminal más a la izquierda
- Evita recursión infinita usando límite de profundidad
- Algoritmo inteligente que prefiere producciones terminales cerca del límite
- Cuando quedan pocos pasos, selecciona producciones que no tienen recursión izquierda

### Mutación Sintáctica:
- 9 tipos diferentes de mutaciones implementadas
- Basadas en errores comunes en parsers y compiladores
- Preservan cierta estructura de la cadena original
- Cada mutación genera un caso inválido específico

### Casos Extremos:
- 6 tipos diferentes de casos extremos
- Buscan los límites del sistema
- Útiles para stress testing
- Cubren edge cases importantes

### Clasificación Automática:
- Cada caso tiene un ID único (VALID_XXXX, INVALID_XXXX, EXTREME_XXXX)
- Metadata completa calculada automáticamente
- Información de operadores, paréntesis, longitud, profundidad

---

## Solución de Problemas

### El botón "Cargar desde archivo" no funciona:
- Verifica que el archivo `gramatica_ejemplo.txt` esté en la carpeta del proyecto
- Revisa que el formato sea correcto (NoTerminal -> Simbolos)
- Asegúrate de que la primera línea no sea comentario (define el símbolo inicial)

### El botón "Exportar JSON" no genera archivo:
- Primero debes generar una suite completa con "Generar Todo"
- El archivo se guarda automáticamente en la carpeta del proyecto
- Busca archivos que empiecen con `casos_prueba_`

### Error de profundidad máxima:
- Aumenta el valor del slider de profundidad máxima
- La gramática de expresiones aritméticas tiene recursión izquierda
- Se recomienda profundidad de 20-50 para resultados óptimos

---

## Autor

Proyecto desarrollado para INFO1148 - Teoría de la Computación  
Semestre II-2025

---

## Licencia

Ver archivo `LICENSE` en el repositorio.

Aplicación de escritorio desarrollada en **C# con Avalonia UI** capaz de generar automáticamente casos de prueba (válidos, inválidos y extremos) a partir de una **Gramática Libre de Contexto (GLC)**.

El sistema implementa derivación formal, mutación sintáctica y generación de casos extremos para testing exhaustivo de parsers, compiladores y validadores.

---

## ✨ Funcionalidades Implementadas

### 1. **Generación de Casos Válidos** ✅
- Derivación leftmost (por la izquierda) desde el símbolo inicial
- Algoritmo inteligente que evita recursión infinita
- Control de profundidad máxima
- Historial paso a paso de la derivación

### 2. **Generación de Casos Inválidos** ✅ (Mutación Sintáctica)
- **Paréntesis desbalanceados**: Elimina o agrega paréntesis
- **Operadores duplicados**: `id + + id`
- **Operador al inicio**: `+ id * id`
- **Operador al final**: `id + id *`
- **Paréntesis vacío**: `( )`
- **Operador faltante**: `id id` (dos operandos consecutivos)
- **Identificador faltante**: `id + * id`
- **Carácter inválido**: `id @ id`
- **Espacios en medio de token**: `i d + id`

### 3. **Generación de Casos Extremos** ✅
- **Profundidad máxima**: Derivación muy profunda
- **Profundidad mínima**: Caso más simple posible
- **Complejidad máxima**: Máximo número de operadores
- **Expresión larga**: Muchos símbolos terminales
- **Expresión corta**: Mínima expresión válida
- **Anidamiento máximo**: Múltiples niveles de paréntesis

### 4. **Sistema de Clasificación Automática** ✅
- Categoriza cada caso: válido/inválido/extremo
- Metadata completa: profundidad, operadores, longitud, etc.
- IDs únicos para cada caso

### 5. **Exportación a JSON** ✅
- Formato estructurado con toda la información
- Incluye gramática utilizada
- Métricas completas del proceso
- Configuración de generación
- Timestamp y versión

### 6. **Sistema de Métricas** ✅
- **Distribución porcentual** por categoría
- **Longitud promedio** de expresiones
- **Profundidad máxima** del árbol sintáctico
- **Operadores generados** por tipo (+, *, -, /)
- **Tipos de mutación** aplicados
- **Tipos de casos extremos** generados
- **Tiempo de ejecución**

### 7. **Parser de Gramáticas desde TXT** ✅
- Carga gramáticas desde archivos de texto
- Formato simple: `E -> E + T`
- Soporta comentarios con `#`
- Detección automática de terminales/no terminales

---

## 🏗️ Arquitectura del Sistema

```
Gramática G (TXT) ──> ParserGramatica
                            │
                            ▼
                   ContextFreeGrammar
                            │
              ┌─────────────┼─────────────┐
              │             │             │
              ▼             ▼             ▼
    GeneradorDerivaciones  Mutador  GeneradorCasosExtremos
    (Casos válidos)    (Inválidos)   (Extremos)
              │             │             │
              └─────────────┼─────────────┘
                            ▼
                     Clasificador
                            │
                            ▼
                    Lista de CasoPrueba
                            │
              ┌─────────────┼─────────────┐
              ▼             ▼             ▼
       GeneradorMetricas  ExportadorJSON  UI
```

---

## 🎯 Casos de Uso

### **Uso 1: Generar 1 Caso Válido con Historial**
1. Ajusta la **profundidad máxima** (default: 20)
2. Click en **"🔍 Caso Válido + Historial"**
3. Ver derivación paso a paso en panel derecho

**Resultado:**
```
Paso 0: E
Paso 1: E + T (aplicando: E → E + T)
Paso 2: T + T (aplicando: E → T)
Paso 3: F + T (aplicando: T → F)
Paso 4: id + T (aplicando: F → id)
Paso 5: id + F (aplicando: T → F)
Paso 6: id + id (aplicando: F → id)
```

### **Uso 2: Generar Múltiples Casos Válidos**
1. Configura **cantidad de casos válidos** (slider)
2. Click en **"✓ Casos Válidos"**
3. Ver lista de expresiones generadas

### **Uso 3: Generar Casos Inválidos**
1. Configura **cantidad de casos inválidos**
2. Click en **"✗ Casos Inválidos"**
3. Ver mutaciones con tipo entre corchetes:
   ```
   [OperadorDuplicado] id + + id
   [ParentesisDesbalanceados] ( id + id
   [CaracterInvalido] id @ id
   ```

### **Uso 4: Generar Suite Completa**
1. Configura cantidades deseadas
2. Click en **"🎯 Generar Todo"**
3. Sistema genera:
   - N casos válidos
   - M casos inválidos
   - K casos extremos
4. Ver **Reporte de Métricas** completo

**Ejemplo de Métricas:**
```
╔═══════════════════════════════════════════════════════════════╗
║          REPORTE DE MÉTRICAS - GENERACIÓN DE CASOS           ║
╚═══════════════════════════════════════════════════════════════╝

📊 RESUMEN GENERAL
─────────────────────────────────────────────────────────────
  Total de casos generados: 21
  Tiempo de ejecución: 245 ms
  Tiempo promedio por caso: 11.67 ms

📈 DISTRIBUCIÓN POR CATEGORÍA
─────────────────────────────────────────────────────────────
  ✓ Válidos:        5 ( 23.8%)
  ✗ Inválidos:      5 ( 23.8%)
  ⚡ Extremos:      11 ( 52.4%)

📏 ESTADÍSTICAS DE LONGITUD (tokens)
─────────────────────────────────────────────────────────────
  Promedio: 7.52
  Mínima:   1
  Máxima:   15

➕ OPERADORES GENERADOS
─────────────────────────────────────────────────────────────
  +   :    12 ( 48.0%)
  *   :    10 ( 40.0%)
  -   :     3 ( 12.0%)
  Total: 25
```

### **Uso 5: Exportar a JSON**
1. Después de generar suite completa
2. Click en **"💾 Exportar JSON"**
3. Archivo se guarda en **carpeta del proyecto**
4. Nombre: `casos_prueba_YYYYMMDD_HHMMSS.json`

**Estructura del JSON:**
```json
{
  "gramatica": {
    "nombre": "Gramática de Expresiones Aritméticas",
    "simboloInicial": "E",
    "variables": ["E", "T", "F"],
    "terminales": ["+", "*", "(", ")", "id"],
    "producciones": ["E → E + T", "E → T", ...]
  },
  "casos": [
    {
      "id": "VALID_0001",
      "cadena": "id + id * id",
      "categoria": "Valido",
      "descripcion": "Caso válido generado por derivación",
      "metadata": {
        "profundidad": 8,
        "num_tokens": 5,
        "total_operadores": 2,
        ...
      }
    }
  ],
  "metricas": {
    "totalCasos": 21,
    "casosValidos": 5,
    "distribucionPorcentual": {...},
    ...
  }
}
```

---

## 🚀 Cómo Ejecutar

### **Requisitos:**
- .NET SDK 8.0 o superior
- Windows (aplicación de escritorio)

### **Pasos:**
```powershell
# 1. Clonar/Descargar el proyecto
cd miniproyecto2_info1148

# 2. Restaurar dependencias
dotnet restore

# 3. Compilar
dotnet build

# 4. Ejecutar
dotnet run
```

---

## 📂 Estructura del Proyecto

```
miniproyecto2_info1148/
├── Models/                          # Lógica de negocio
│   ├── ContextFreeGrammar.cs       # Definición formal de GLC
│   ├── GeneradorDerivaciones.cs    # Derivación leftmost
│   ├── Mutador.cs                  # Generación de casos inválidos
│   ├── GeneradorCasosExtremos.cs   # Generación de edge cases
│   ├── Clasificador.cs             # Clasificación automática
│   ├── ExportadorJSON.cs           # Exportación a JSON
│   ├── GeneradorMetricas.cs        # Cálculo de estadísticas
│   ├── ParserGramatica.cs          # Parser de archivos TXT
│   ├── Symbol.cs                   # Clase base de símbolos
│   ├── Terminal.cs                 # Símbolos terminales
│   ├── NonTerminal.cs              # Símbolos no terminales
│   ├── Production.cs               # Reglas de producción
│   └── GramaticaExpresionesAritmeticas.cs  # Gramática ejemplo
├── ViewModels/                      # Lógica de presentación
│   └── MainWindowViewModel.cs
├── Views/                           # Interfaz gráfica
│   └── MainWindow.axaml
└── README.md                        # Este archivo
```

---

## 🎨 Interfaz de Usuario

### **Panel Izquierdo - Configuración:**
- Slider de profundidad máxima (5-100)
- Slider de casos válidos (1-20)
- Slider de casos inválidos (1-20)
- Botones de acciones rápidas
- Botones de suite completa

### **Panel Derecho - Resultados:**
- Resumen de generación
- Lista de casos generados
- Historial de derivación (cuando aplica)
- Reporte de métricas (suite completa)

---

## 📊 Métricas Calculadas

1. **Cantidad total** de casos generados
2. **Distribución porcentual** (válidos/inválidos/extremos)
3. **Longitud promedio** de expresiones
4. **Profundidad máxima** de derivación
5. **Operadores por tipo** (+, *, -, /)
6. **Niveles de mutación** aplicados
7. **Tipos de casos extremos** generados
8. **Tiempo de ejecución** y tiempo promedio por caso

---

## 🧪 Ejemplo de Gramática TXT

Crea un archivo `gramatica.txt`:

```txt
# Gramática de Expresiones Aritméticas
# Líneas con # son comentarios

E -> E + T
E -> T
T -> T * F
T -> F
F -> ( E )
F -> id
```

---

## 🔧 Tecnologías Utilizadas

- **Lenguaje:** C# 12
- **Framework:** .NET 8.0
- **UI:** Avalonia UI 11.3.8
- **MVVM:** CommunityToolkit.Mvvm 8.2.1
- **JSON:** System.Text.Json

---

## 📝 Notas Técnicas

### **Derivación Leftmost:**
- Expande siempre el no terminal más a la izquierda
- Evita recursión infinita usando límite de profundidad
- Algoritmo inteligente que prefiere producciones terminales cerca del límite

### **Mutación Sintáctica:**
- 9 tipos diferentes de mutaciones
- Basadas en errores comunes en parsers
- Preservan cierta estructura de la cadena original

### **Casos Extremos:**
- Buscan límites del sistema
- Útiles para stress testing
- Cubren edge cases importantes

---

## 👨‍💻 Autor

Proyecto desarrollado para INFO1148 - Teoría de la Computación
Semestre II-2025

---

## 📄 Licencia

Ver archivo `LICENSE` en el repositorio.
