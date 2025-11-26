using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using miniproyecto2_info1148.Models;

namespace miniproyecto2_info1148.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // Propiedades observables
    [ObservableProperty]
    private string _gramaticaInfo = string.Empty;

    [ObservableProperty]
    private int _profundidadMaxima = 20;

    [ObservableProperty]
    private int _cantidadCadenas = 5;

    [ObservableProperty]
    private int _cantidadInvalidos = 5;

    [ObservableProperty]
    private int _cantidadExtremos = 10;

    [ObservableProperty]
    private string _resultadosGeneracion = string.Empty;

    [ObservableProperty]
    private string _historialDerivacion = string.Empty;

    [ObservableProperty]
    private string _reporteMetricas = string.Empty;

    [ObservableProperty]
    private bool _mostrarHistorial = false;

    [ObservableProperty]
    private bool _mostrarMetricas = false;

    [ObservableProperty]
    private int _longitudMaxima = 100;

    [ObservableProperty]
    private string _archivoGramaticaCargado = "Gramática predeterminada";

    // Colecciones observables
    public ObservableCollection<string> CadenasGeneradas { get; } = new();
    
    /// <summary>
    /// Colección observable de casos de prueba para el DataGrid.
    /// Incluye: cadena, categoría, longitud, profundidad y métricas asociadas.
    /// </summary>
    public ObservableCollection<CasoPruebaViewModel> CasosPrueba { get; } = new();

    // Backend
    private GramaticaExpresionesAritmeticas? _gramaticaDefault;
    private ContextFreeGrammar? _gramaticaActual;
    private GeneradorDerivaciones? _generador;
    private Mutador? _mutador;
    private GeneradorCasosExtremos? _generadorExtremos;
    private Clasificador? _clasificador;
    private GeneradorMetricas? _generadorMetricas;
    private ExportadorJSON? _exportador;
    private List<CasoPrueba> _todosLosCasos = new();

    public MainWindowViewModel()
    {
        InicializarComponentes();
        InicializarGramatica();
    }

    /// <summary>
    /// Inicializa los componentes del sistema.
    /// </summary>
    private void InicializarComponentes()
    {
        _mutador = new Mutador();
        _clasificador = new Clasificador();
        _generadorMetricas = new GeneradorMetricas();
        _exportador = new ExportadorJSON();
    }

    /// <summary>
    /// Inicializa la gramática de expresiones aritméticas.
    /// </summary>
    private void InicializarGramatica()
    {
        _gramaticaDefault = new GramaticaExpresionesAritmeticas();
        _gramaticaActual = _gramaticaDefault.Gramatica;
        GramaticaInfo = _gramaticaDefault.ToString();
    }

    /// <summary>
    /// Comando para generar una sola cadena con historial detallado.
    /// </summary>
    [RelayCommand]
    private void GenerarCadenaConHistorial()
    {
        if (_gramaticaActual == null) return;

        try
        {
            _generador = new GeneradorDerivaciones(_gramaticaActual, ProfundidadMaxima);
            var cadena = _generador.GenerarCadena();

            ResultadosGeneracion = $"✅ Cadena generada:\n{cadena}\n\n" +
                                   $"Pasos de derivación: {_generador.HistorialDerivacion.Count}";

            HistorialDerivacion = _generador.ObtenerHistorialTexto();
            MostrarHistorial = true;
            MostrarMetricas = false;
        }
        catch (Exception ex)
        {
            ResultadosGeneracion = $"❌ Error al generar cadena:\n{ex.Message}";
            HistorialDerivacion = string.Empty;
            MostrarHistorial = false;
        }
    }

    /// <summary>
    /// Comando para generar múltiples cadenas.
    /// </summary>
    [RelayCommand]
    private void GenerarMultiplesCadenas()
    {
        if (_gramaticaActual == null) return;

        try
        {
            _generador = new GeneradorDerivaciones(_gramaticaActual, ProfundidadMaxima);
            CadenasGeneradas.Clear();

            var cadenas = _generador.GenerarMultiplesCadenas(CantidadCadenas);

            foreach (var cadena in cadenas)
            {
                CadenasGeneradas.Add(cadena);
            }

            ResultadosGeneracion = $"✅ Se generaron {cadenas.Count} cadenas válidas exitosamente.\n\n" +
                                   $"Profundidad máxima: {ProfundidadMaxima}\n" +
                                   $"Cadenas solicitadas: {CantidadCadenas}";

            MostrarHistorial = false;
            MostrarMetricas = false;
        }
        catch (Exception ex)
        {
            ResultadosGeneracion = $"❌ Error al generar cadenas:\n{ex.Message}";
            MostrarHistorial = false;
        }
    }

    /// <summary>
    /// Comando para generar casos inválidos (mutación).
    /// </summary>
    [RelayCommand]
    private void GenerarCasosInvalidos()
    {
        if (_gramaticaActual == null || _mutador == null) return;

        try
        {
            CadenasGeneradas.Clear();
            
            // Primero generar una cadena válida
            _generador = new GeneradorDerivaciones(_gramaticaActual, ProfundidadMaxima);
            var cadenaValida = _generador.GenerarCadena();

            // Mutar para crear casos inválidos
            var casosInvalidos = _mutador.GenerarCasosInvalidos(cadenaValida, CantidadInvalidos);

            foreach (var caso in casosInvalidos)
            {
                CadenasGeneradas.Add($"[{caso.TipoMutacion}] {caso.Cadena}");
            }

            ResultadosGeneracion = $"✅ Se generaron {casosInvalidos.Count} casos inválidos por mutación.\n\n" +
                                   $"Cadena original: {cadenaValida}\n" +
                                   $"Tipos de mutación aplicados: {casosInvalidos.Count}";

            MostrarHistorial = false;
            MostrarMetricas = false;
        }
        catch (Exception ex)
        {
            ResultadosGeneracion = $"❌ Error al generar casos inválidos:\n{ex.Message}";
        }
    }

    /// <summary>
    /// Comando para generar casos extremos.
    /// </summary>
    [RelayCommand]
    private void GenerarCasosExtremos()
    {
        if (_gramaticaActual == null) return;

        try
        {
            CadenasGeneradas.Clear();
            _generadorExtremos = new GeneradorCasosExtremos(_gramaticaActual);
            
            var casosExtremos = _generadorExtremos.GenerarCasosExtremos(cantidadPorTipo: 2);

            foreach (var caso in casosExtremos)
            {
                CadenasGeneradas.Add($"[{caso.Tipo}] {caso.Cadena}");
            }

            ResultadosGeneracion = $"✅ Se generaron {casosExtremos.Count} casos extremos.\n\n" +
                                   $"Tipos generados: profundidad máxima/mínima, complejidad, anidamiento, etc.";

            MostrarHistorial = false;
            MostrarMetricas = false;
        }
        catch (Exception ex)
        {
            ResultadosGeneracion = $"❌ Error al generar casos extremos:\n{ex.Message}";
        }
    }

    /// <summary>
    /// Comando para generar suite completa de casos de prueba.
    /// </summary>
    [RelayCommand]
    private async Task GenerarSuiteCompleta()
    {
        if (_gramaticaActual == null || _clasificador == null || _generadorMetricas == null) return;

        try
        {
            _generadorMetricas.IniciarMedicion();
            _todosLosCasos.Clear();
            _clasificador.ReiniciarContador();

            ResultadosGeneracion = "⏳ Generando suite completa de casos de prueba...\n";

            // 1. Generar casos válidos
            _generador = new GeneradorDerivaciones(_gramaticaActual, ProfundidadMaxima);
            var cadenasValidas = _generador.GenerarMultiplesCadenas(CantidadCadenas);
            var casosValidos = _clasificador.ClasificarCasosValidos(cadenasValidas, _generador);
            _todosLosCasos.AddRange(casosValidos);

            // 2. Generar casos inválidos
            if (cadenasValidas.Count > 0 && _mutador != null)
            {
                var cadenaBase = cadenasValidas.First();
                var casosInvalidos = _mutador.GenerarCasosInvalidos(cadenaBase, CantidadInvalidos);
                foreach (var ci in casosInvalidos)
                {
                    _todosLosCasos.Add(_clasificador.ClasificarCasoInvalido(ci));
                }
            }

            // 3. Generar casos extremos
            _generadorExtremos = new GeneradorCasosExtremos(_gramaticaActual);
            var casosExtremos = _generadorExtremos.GenerarCasosExtremos(cantidadPorTipo: 1);
            foreach (var ce in casosExtremos)
            {
                _todosLosCasos.Add(_clasificador.ClasificarCasoExtremo(ce));
            }

            _generadorMetricas.DetenerMedicion();

            // Generar métricas y reporte
            var reporte = _generadorMetricas.GenerarReporte(_todosLosCasos);
            ReporteMetricas = _generadorMetricas.GenerarReporteTexto(reporte);
            
            ResultadosGeneracion = $"✅ Suite completa generada exitosamente!\n\n" +
                                   $"Total de casos: {_todosLosCasos.Count}\n" +
                                   $"• Válidos: {reporte.CasosValidos}\n" +
                                   $"• Inválidos: {reporte.CasosInvalidos}\n" +
                                   $"• Extremos: {reporte.CasosExtremos}\n\n" +
                                   $"Tiempo: {reporte.TiempoEjecucionMs} ms";

            MostrarHistorial = false;
            MostrarMetricas = true;

            // Mostrar algunos ejemplos en lista
            CadenasGeneradas.Clear();
            foreach (var caso in _todosLosCasos.Take(20))
            {
                CadenasGeneradas.Add($"[{caso.Categoria}] {caso.Cadena}");
            }

            // Actualizar DataGrid con todos los casos
            CasosPrueba.Clear();
            foreach (var caso in _todosLosCasos)
            {
                CasosPrueba.Add(new CasoPruebaViewModel(caso));
            }
        }
        catch (Exception ex)
        {
            ResultadosGeneracion = $"❌ Error al generar suite completa:\n{ex.Message}";
            MostrarMetricas = false;
        }
    }

    /// <summary>
    /// Comando para exportar casos a JSON.
    /// </summary>
    [RelayCommand]
    private async Task ExportarJSON()
    {
        if (_todosLosCasos.Count == 0)
        {
            ResultadosGeneracion = "⚠️ No hay casos para exportar. Genera una suite completa primero.";
            return;
        }

        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var nombreArchivo = $"casos_prueba_{timestamp}.json";
            var rutaProyecto = AppDomain.CurrentDomain.BaseDirectory;
            var rutaArchivo = Path.Combine(rutaProyecto, nombreArchivo);

            _exportador?.ExportarACaso(
                _todosLosCasos,
                _gramaticaActual!,
                rutaArchivo,
                new Dictionary<string, object>
                {
                    { "profundidad_maxima", ProfundidadMaxima },
                    { "cantidad_validos", CantidadCadenas },
                    { "cantidad_invalidos", CantidadInvalidos },
                    { "cantidad_extremos", CantidadExtremos }
                },
                _generadorMetricas?.ObtenerTiempoMs() ?? 0
            );

            ResultadosGeneracion = $"✅ Casos exportados exitosamente!\n\n" +
                                   $"Archivo: {nombreArchivo}\n" +
                                   $"Ubicación: Carpeta del proyecto\n" +
                                   $"Total de casos: {_todosLosCasos.Count}";
        }
        catch (Exception ex)
        {
            ResultadosGeneracion = $"❌ Error al exportar JSON:\n{ex.Message}";
        }
    }

    /// <summary>
    /// Comando para cargar gramática desde archivo.
    /// </summary>
    [RelayCommand]
    private async Task CargarGramatica()
    {
        try
        {
            // Intentar cargar desde carpeta Assets primero
            var rutaProyecto = AppDomain.CurrentDomain.BaseDirectory;
            var rutaArchivo = Path.Combine(rutaProyecto, "Assets", "gramatica_ejemplo.txt");

            if (File.Exists(rutaArchivo))
            {
                var parser = new ParserGramatica();
                _gramaticaActual = parser.CargarDesdeArchivo(rutaArchivo);
                
                GramaticaInfo = _gramaticaActual.ToString();
                ArchivoGramaticaCargado = "Assets/gramatica_ejemplo.txt";
                
                ResultadosGeneracion = $"✅ Gramática cargada exitosamente!\n\n" +
                                       $"Archivo: Assets/gramatica_ejemplo.txt\n" +
                                       $"Variables: {_gramaticaActual.Variables.Count}\n" +
                                       $"Terminales: {_gramaticaActual.Terminales.Count}\n" +
                                       $"Producciones: {_gramaticaActual.Producciones.Count}";
            }
            else
            {
                ResultadosGeneracion = "ℹ️ Para cargar una gramática personalizada:\n\n" +
                                       "1. Crea un archivo 'gramatica_ejemplo.txt' en:\n" +
                                       $"   {Path.Combine(rutaProyecto, "Assets")}\n\n" +
                                       "2. Formato del archivo:\n" +
                                       "   E -> E + T\n" +
                                       "   E -> T\n" +
                                       "   T -> T * F\n" +
                                       "   T -> F\n" +
                                       "   F -> ( E )\n" +
                                       "   F -> id\n\n" +
                                       "3. Convenciones:\n" +
                                       "   • Mayúsculas = No terminales\n" +
                                       "   • Minúsculas/símbolos = Terminales\n" +
                                       "   • Usa '->' para separar lados\n" +
                                       "   • Un espacio entre símbolos\n" +
                                       "   • Líneas con # son comentarios\n\n" +
                                       "Por ahora, se usa la gramática predeterminada de expresiones aritméticas.";
            }
        }
        catch (Exception ex)
        {
            ResultadosGeneracion = $"❌ Error al cargar gramática:\n{ex.Message}\n\n" +
                                   "Revisa el formato del archivo. Debe ser:\n" +
                                   "NoTerminal -> Simbolo1 Simbolo2 ...\n\n" +
                                   "Ejemplo válido:\n" +
                                   "E -> E + T\n" +
                                   "E -> T";
            
            // Volver a la gramática predeterminada
            InicializarGramatica();
        }
    }

    /// <summary>
    /// Comando para limpiar resultados.
    /// </summary>
    [RelayCommand]
    private void LimpiarResultados()
    {
        ResultadosGeneracion = string.Empty;
        HistorialDerivacion = string.Empty;
        ReporteMetricas = string.Empty;
        CadenasGeneradas.Clear();
        CasosPrueba.Clear();
        _todosLosCasos.Clear();
        MostrarHistorial = false;
        MostrarMetricas = false;
    }
}
