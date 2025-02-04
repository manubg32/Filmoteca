using System.Data;
using Avalonia.Controls;
using CatalogoAvalonia.ViewModel;

namespace CatalogoAvalonia.View;

public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();
    }

    //Metodo que se ejecuta al pulsar en el boton de cerrar la ventana
    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        Datos d = Datos.getDatos();
        d.guardarDatos();
    }
}