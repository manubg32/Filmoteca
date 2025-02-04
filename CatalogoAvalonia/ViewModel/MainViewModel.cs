using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CatalogoAvalonia.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Color = Avalonia.Media.Color;

namespace CatalogoAvalonia.ViewModel;

public partial class MainViewModel : ObservableObject
{
    //Creamos una variable statica de la clase que contiene las operaciones globales de la aplicacion
    private static Datos d = Datos.getDatos();

    //Creamos la lista sobre la que trabajaremos
    private static List<Pelicula> peliculas = d.getLista();
    
    //Creamos la posicion global que determinara el elemento que se muestra
    public static int _pos;

    //Creamos la ruta a la imagen por defecto
    [ObservableProperty] private string _rutaDefault = "../../../Resources/Carteles/default.jpg";
    
    //Creamos las propiedades observables de la intefaz principal
    [ObservableProperty] private string _titulo;
    [ObservableProperty] private string _director;
    [ObservableProperty] private int _year;
    [ObservableProperty] private int _duracion;
    [ObservableProperty] private Bitmap _foto;
    
    //Creamos las propiedades observables de la intefaz de altas
    [ObservableProperty] private string _nuevoTitulo;
    [ObservableProperty] private string _nuevoDirector;
    [ObservableProperty] private string _nuevoYear;
    [ObservableProperty] private string _nuevaDuracion;
    [ObservableProperty] private Bitmap _nuevaFoto;
    
    //Creamos las propiedades que muestran al usuario el elemento en el que se encuentra
    [ObservableProperty] private int _posicion;
    [ObservableProperty] private int _total;

    //Creamos las propiedades de los botones de movimiento y de borrado
    [ObservableProperty] private bool _btnAnteriorEnabled;
    [ObservableProperty] private bool _btnSiguienteEnabled;
    [ObservableProperty] private bool _btnEliminarEnabled;
    
    //Creamos las propiedades booleanas que mostraran una pantalla u otra
    [ObservableProperty] private bool _pantallaAltaVisible;
    [ObservableProperty] private bool _pantallaPrincipalVisible;

    //Creamos propiedades del color de los bordes para cuando demos de alta un elemento indicar al usuario si hay problemas
    [ObservableProperty] private Brush _bordeTitulo;
    [ObservableProperty] private Brush _bordeDirector;
    [ObservableProperty] private Brush _bordeYear;
    [ObservableProperty] private Brush _bordeDuracion;
    
    //El metodo constructor de la clase hara diferentes gestiones
    public MainViewModel()
    {
        initComponents();                           //Inicializamos la interfaz grafica
        //cargarPeliculas;                          //Metódo de prueba que utilizaba para cargar las peliculas directamente
        mostrarPrimero();                           //Mostramos el primer elemento cargado de la lista
        NuevaFoto = new Bitmap(RutaDefault);        //La foto que se muestra por defecto la cargamos
        Total = peliculas.Count;                    //Obtenemos el total de peliculas para indicar al usuario
    }
    
    //Mostramos la pantalla principal al iniciar la interfaz grafica
    private void initComponents()
    {
        PantallaAltaVisible = false;
        PantallaPrincipalVisible = true;
    }
    
    //Metodo que muestra el primer elemento de la lista
    private void mostrarPrimero()
    {
        _pos = 0;                                           //Ponemos la posicion a 0
        Pelicula primera = peliculas.ElementAt(_pos);       //Seleccionamos la primera pelicula
        
        //Enlazamos los datos con los elementos visibles
        Titulo = primera.Titulo;
        Director = primera.Director;
        Year = primera.Year;
        Duracion = primera.Duracion;
        Foto = ByteToBitMap(primera.FotoBin);
        
        //Comprobamos los botones
        comprobarBotones();
    }
    
    //Comando que se ejecuta al pulsar el boton Siguiente
    [RelayCommand]
    private void Siguiente()
    {
        _pos++;                                         //Incrementamos la posicion
        Pelicula p = peliculas.ElementAt(_pos);         //Seleccionamos el elemento
        
        //Enlazamos los datos con los elementos visibles
        Titulo = p.Titulo;
        Director = p.Director;
        Year = p.Year;
        Duracion = p.Duracion;
        Foto = ByteToBitMap(p.FotoBin);

        //Comprobamos los botones
        comprobarBotones();
    }
    
    //Comando que se ejecuta al pulsar el boton Anterior
    [RelayCommand]
    private void Anterior()
    {
        _pos--;                                     //Decrementamos la posicion
        Pelicula p = peliculas.ElementAt(_pos);     //Seleccionamos el elemento
        
        //Enlazamos los datos con los elementos visibles
        Titulo = p.Titulo;
        Director = p.Director;
        Year = p.Year;
        Duracion = p.Duracion;
        Foto = ByteToBitMap(p.FotoBin);

        //Comprobamos los botones
        comprobarBotones();
    }
    
    //Comando que se ejecuta al pulsar el boton Eliminar
    [RelayCommand]
    private void Eliminar()
    {
        
        peliculas.RemoveAt(_pos);           //Eliminamos el elemento que coincida con la posicion
        
        if (_pos > 0)
        {
            _pos = _pos - 1;                //Siempre que la posicion sea mayor que 0
        }
        
        Total = peliculas.Count;            //Actualizamos el total de elementos que se le muestra al usuario            
        comprobarBotones();                 //Comprobamos los botones
        mostrarActual();                    //Mostramos el elemento actual (_pos)
    }
    
    //Comando que se ejecuta al pulsar el boton Alta
    [RelayCommand]
    private void Alta()
    {
        _pos = 0;                               //Ponemos la posicion a 0 para cuando volvamos a la pantalla principal
        
        PantallaPrincipalVisible = false;       //Ocultamos la "pantalla principal"
        
        //Ponemos los campos vacíos para que se muestren los hints (watermark)
        NuevoTitulo = "";
        NuevoDirector = "";
        NuevoYear = "";
        NuevaDuracion = "";
        
        //Mostramos la pantalla de Altas
        PantallaAltaVisible = true;
    }
    
    //Comando que se ejecuta al pulsar sobre el boton Guardar
    [RelayCommand]
    private void Guardar()
    {
        //Creamos variables para almacenar lo introducido en los campos
        String nuevoTitulo = NuevoTitulo;
        String nuevoDirector = NuevoDirector;
        int nuevoYear;
        int nuevaDuracion;
        Bitmap nuevaFoto = NuevaFoto;

        //Hacemos una comprobación de los campos ya sea con if/else o forzamos un try/catch si hay algun error
        try
        {
            //Comprobamos que el año introducido sea un Int
            nuevoYear = Int32.Parse(NuevoYear);
            
            //Si es un numero ponemos los bordes a negro
            BordeYear = new SolidColorBrush(Colors.Black);
            try
            {
                //Comprobamos que la duracion introducida sea un Int
                nuevaDuracion = Int32.Parse(NuevaDuracion);
                
                //Si es un numero ponemos los bordes a negro
                BordeDuracion = new SolidColorBrush(Colors.Black);
                
                if (!String.IsNullOrEmpty(nuevoTitulo)) //Comprobamos que el titulo no este vacio
                {
                    BordeTitulo = new SolidColorBrush(Colors.Black); //Ponemos el borde a negro
                    
                    if (!String.IsNullOrEmpty(nuevoDirector)) //Comprobamos que el director no este vacio
                    {
                        BordeDirector = new SolidColorBrush(Colors.Black); //Ponemos el borde a negro
                        
                        if (NuevaFoto == null) //Si no hay foto
                        {
                            nuevaFoto = new Bitmap(RutaDefault); //Ponemos la foto por defecto
                        }
                        else
                        {
                            nuevaFoto = NuevaFoto; //Si hay foto ponemos la foto introducida
                        }
                        
                        //Una vez hecha las comprobaciones creamos el objeto Pelicula
                        Pelicula p1 = new Pelicula(nuevoTitulo, nuevoDirector, nuevoYear, nuevaDuracion, BitMaptoByte(nuevaFoto));
                        
                        peliculas.Add(p1);                      //Agregamos el objeto a la lista 
                        PantallaPrincipalVisible = true;        //Mostramos la pantalla principal
                        PantallaAltaVisible = false;            //Ocultamos la pantalla de altas

                        mostrarPrimero();                       //Mostramos el primer elemento
                        NuevaFoto = new Bitmap(RutaDefault);    //Volvemos a poner la foto por defecto para cuando se muestre la pantalla altas otra vez
                    }
                    else
                    {
                        //Si no es lo esperado ponemos el borde a rojo para indicarselo al usuario
                        BordeDirector = new SolidColorBrush(Colors.Red);
                    }
                }
                else
                {
                    //Si no es lo esperado ponemos el borde a rojo para indicarselo al usuario
                    BordeTitulo = new SolidColorBrush(Colors.Red);
                }
            }
            catch (FormatException e)
            {
                //Si no es lo esperado ponemos el borde a rojo para indicarselo al usuario
                BordeDuracion = new SolidColorBrush(Colors.Red);
            }
        }
        catch (FormatException e)
        {
            //Si no es lo esperado ponemos el borde a rojo para indicarselo al usuario
            BordeYear = new SolidColorBrush(Colors.Red);
        }
    }
    
    //Comando que se ejecuta al pulsar sobre la foto en la pantalla de altas para seleccionar una
    [RelayCommand]
    private async void ElegirFoto(Window ventanaPadre)
    {
        try
        {
            //Abrimos un dialog que permita extensiones jpg, png, y todos y permitimos solo una opcion
            var dlg = new OpenFileDialog();
            dlg.Filters.Add(new FileDialogFilter() { Name = "Imágenes JPEG", Extensions = { "jpg" } });
            dlg.Filters.Add(new FileDialogFilter() { Name = "Imágenes PNG", Extensions = { "png" } });
            dlg.Filters.Add(new FileDialogFilter() { Name = "Todos los archivos", Extensions = { "*" } });
            dlg.AllowMultiple = false;

            //Obtenemos el resultado de esa pantalla
            var result = await dlg.ShowAsync(ventanaPadre);
          
            //Si el resultado no es nulo:
            if (result != null)
            {
                string rutaFoto = result[0];                    //Obtenemos la ruta del resultado obtenido
                byte[] fotoBin = File.ReadAllBytes(rutaFoto);   //Leemos todos los bytes del archivo indicado
                Stream st = new MemoryStream(fotoBin);          //Creamos un Stream con los datos del array de bytes
                Bitmap bitmap = new Bitmap(st);                 //Creamos el Bitmap a raiz del Stream
                NuevaFoto = new Bitmap(rutaFoto);               //Mostramos la foto
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);                      //Si hay algun error se muestra por consola
        }
        
    }

    //Comando que se ejecuta al pulsar sobre el boton Cancelar
    [RelayCommand]
    private void Cancelar()
    {
        PantallaPrincipalVisible = true;        //Mostramos la pantalla principal
        PantallaAltaVisible = false;            //Ocultamos la pantalla de altas
        mostrarPrimero();                       //Mostramos el primer elemento
    }

    //Metodo que muestra el elememto que corresponde a la posicion
    private void mostrarActual()
    {
        Pelicula primera = peliculas.ElementAt(_pos);       //Seleccionamos la pelicula que esta en la posicion
        
        //Enlazamos los datos con los elementos visibles
        Titulo = primera.Titulo;                
        Director = primera.Director;
        Year = primera.Year;
        Duracion = primera.Duracion;
        Foto = ByteToBitMap(primera.FotoBin);

        //Comprobamos los botones
        comprobarBotones();
    }
    

    //Metodo que convierte desde un array de Bytes a Bitmap
    private Bitmap ByteToBitMap(byte[] fotoBin)
    {
        Stream st = new MemoryStream(fotoBin);
        Bitmap bitmap = new Bitmap(st);
        return bitmap;
    }
    
    //Metodo que convierte desde un Bitmap a array de Bytes
    private byte[] BitMaptoByte(Bitmap bitmap)
    {
        MemoryStream ms = new MemoryStream();
        bitmap.Save(ms);
        return ms.ToArray();
    }

    //Metodo que comprueba los botones para activarlos segun la posicion
    private void comprobarBotones()
    {
        Posicion = _pos + 1;            //La posicion que se le muestra al usuario
        Total = peliculas.Count;        //El total de películas que se le muestra al usuario
        
        
        //Si la posicion es cero desactivamos el boton anterior, si no lo activamos
        if (_pos == 0)                  
        {
            BtnAnteriorEnabled = false;
        }
        else
        {
            BtnAnteriorEnabled = true;
        }

        //Si la posicion es el count-1 desactivamos el boton siguiente, si no lo activamos
        if (_pos == peliculas.Count - 1)
        {
            BtnSiguienteEnabled = false;
        }
        else
        {
            BtnSiguienteEnabled = true;
        }

        //Si solo queda un elemento en la lista desactivamos el boton borrar, si no lo activamos
        if (peliculas.Count <= 1)
        {
            BtnEliminarEnabled = false;
        }
        else
        {
            BtnEliminarEnabled = true;
        }
    }

    //Metodo para pruebas que insertaba cuatro peliculas
    private void cargarPeliculas()
    {
        Pelicula p1 = new Pelicula("Spiderman", "Sam Raimi", 2002, 121, BitMaptoByte(new Bitmap("..\\..\\..\\Resources\\Carteles\\spiderman.jpg")));
        Pelicula p2 = new Pelicula("Metropolis", "Fritz Lang", 1927, 153, BitMaptoByte(new Bitmap("..\\..\\..\\Resources\\Carteles\\metropolis.jpg")));
        Pelicula p3 = new Pelicula("12 Angry Men", "Sidney Lumet", 1957, 96, BitMaptoByte(new Bitmap("..\\..\\..\\Resources\\Carteles\\12hombressinpiedad.jpg")));
        Pelicula p4 = new Pelicula("They Live", "John Carpenter", 1988, 94, BitMaptoByte(new Bitmap("..\\..\\..\\Resources\\Carteles\\theylive.png")));
        
        peliculas.Add(p1);
        peliculas.Add(p2);
        peliculas.Add(p3);
        peliculas.Add(p4);
    }

    
}