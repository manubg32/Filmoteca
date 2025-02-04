using System;
using Avalonia.Media.Imaging;

namespace CatalogoAvalonia.Model;

//Clase Pelicula, es Serializable para poder guardar la lista facil
[Serializable]
public class Pelicula 
{
    
    public byte[] FotoBin {get; set;}               //Atributo que guarda la foto
    
    public int Duracion {get; set;}                 //Atributo que guarda la duracion

    public int Year {get; set;}                     //Atributo que guarda el año

    public String Director {get; set;}              //Atributo que guarda el director

    public String Titulo {get; set;}                //Atributo que guarda el titulo
   
    //Metodo constructor con todos los atributos
    public Pelicula(String titulo, String director, int year, int duracion, byte[] foto)
    {
        Titulo = titulo;
        Director = director;
        Year = year;
        Duracion = duracion;
        FotoBin = foto;
    }

    //Metodo constructor vacio por si fuera necesario
    public Pelicula(){}

}