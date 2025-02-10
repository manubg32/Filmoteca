using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CatalogoAvalonia.Model;

namespace CatalogoAvalonia.ViewModel;

//Clase con elementos y metodos generales para utilizar, sigue el patron SingleTone
public class Datos
{
    private static Datos _instance;     //Instancia de la clase para tener solo una instancia
    private List<Pelicula> peliculas;   //Lista de peliculas real
    
    private string path = "../../../../peliculas.json"; //Ruta para que no se guarde el json en bin

    //Metodo que devuelve la lista
    public List<Pelicula> getLista()
    {
        return peliculas;
    }

    //Metodo que crea una sola instancia y la da al usuario (patron SingleTone)
    public static Datos getDatos()
    {
        if (_instance == null)
        {
            _instance = new Datos();
        }

        return _instance;
    }

    //Metodo constructor que crea la lista y carga los datos
    private Datos()
    {
        peliculas = new List<Pelicula>();
        cargarDatos();
    }

    //Metodo que guarda los datos (la lista serializable)
    public void guardarDatos()
    {
        string json = JsonSerializer.Serialize(peliculas);
        File.WriteAllText(path, json);
    }

    //Metodo que carga los datos (la lista serializable)
    public void cargarDatos()
    {   
        string jsonLeido = File.ReadAllText(path);
        peliculas = JsonSerializer.Deserialize<List<Pelicula>>(jsonLeido);
    }

}