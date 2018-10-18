using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Models;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ANVI6MLNetCoreTechint
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static string datapath = @"datos.txt";
        public static string testDataPath = @"datosTest.txt";
        public static string _modelpath = @"D:\Model.zip";
        private static PredictionModel<DatosGenerales, Prediccion> model;
        public List<Destino> tuplesList = new List<Destino>();
        public List<Categorias> tuplesListCategorias = new List<Categorias>();
        public class DatosGenerales
        {
            [Column("0")]
            public float Peso;
            [Column("1")]
            public float Destino;
            [Column("2")]
            public float Categoria;
            [Column("3")]
            public float TiempoEntrega;




        }
        public class Prediccion
        {
            [ColumnName("Score")]
            public float TiempoEntrega;

        }
        
        public ObservableCollection<Destino> categorias = new ObservableCollection<Destino>();
        public MainPage()
        {
            this.InitializeComponent();
            tuplesList.Add(new Destino() { DestinoItem = "CABA", DestinoId=1 });
            tuplesList.Add(new Destino() { DestinoItem = "Cordoba", DestinoId=2 });
            tuplesList.Add(new Destino() { DestinoItem = "BsAs", DestinoId=3 });
            tuplesList.Add(new Destino() { DestinoItem = "Campana", DestinoId=4 });
            tuplesList.Add(new Destino() { DestinoItem = "Misiones", DestinoId=5 });

            tuplesListCategorias.Add(new Categorias() { CategoriaItem = "Categoria 1", CategoriaId=1 });
            tuplesListCategorias.Add(new Categorias() { CategoriaItem = "Categoria 2", CategoriaId=2 });
            tuplesListCategorias.Add(new Categorias() { CategoriaItem = "Categoria 3", CategoriaId=3 });
            tuplesListCategorias.Add(new Categorias() { CategoriaItem = "Categoria 4", CategoriaId=4 });
            tuplesListCategorias.Add(new Categorias() { CategoriaItem = "Categoria 5",  CategoriaId=5 });
        }
        public static async Task<PredictionModel<DatosGenerales, Prediccion>> Train()
        {
            
            try
            {

                var pipeline = new LearningPipeline();
                pipeline.Add(new Microsoft.ML.Data.TextLoader(datapath).CreateFrom<DatosGenerales>(useHeader: true, separator: ','));
                pipeline.Add(new ColumnCopier(("TiempoEntrega", "Label")));
                pipeline.Add(new ColumnConcatenator(outputColumn: "Features", "Peso", "Destino", "Categoria"));
                pipeline.Add(new LightGbmRegressor() { NumLeaves = 5, NumBoostRound = 5, MinDataPerLeaf = 2 });
                PredictionModel<DatosGenerales, Prediccion> model = pipeline.Train<DatosGenerales, Prediccion>();

                Windows.Storage.StorageFolder installedLocation = Windows.ApplicationModel.Package.Current.InstalledLocation;
                var streamGenerado = generarStream(model);

                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", installedLocation);
                var pathStr = @"model.zip";
               

                await model.WriteAsync(streamGenerado);
                                
                //int bufferSize = 1024;
                //byte[] buffer = new byte[bufferSize];
                //int readCount = await streamGenerado.ReadAsync(buffer, 0, bufferSize);

                //using (FileStream DestinationStream = File.Create(pathStr,(int)streamGenerado.Length,FileOptions.WriteThrough))
                //{
                //    await streamGenerado.CopyToAsync(DestinationStream);
                //}


                return model;
            }
            catch (Exception ex)
            {
                var error = ex.Message.ToString();

                throw;
            }

        }
        public static Stream generarStream(PredictionModel<DatosGenerales, Prediccion> predictionModel)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write((object)predictionModel);
            writer.Flush();
            stream.Position = 0;

            return stream;
        }
        private void btnPredecir_Click(object sender, RoutedEventArgs e)
        {

        }
        
        private async void BtnEntrenar_Click(object sender, RoutedEventArgs e)
        {
           

            model = await Train();
            Evaluate(model);
            var destinoSel = ((ANVI6MLNetCoreTechint.Destino)lstBoxDestino.SelectedItem).DestinoId;
            var categoriasSel = ((ANVI6MLNetCoreTechint.Categorias)lstBoxCat.SelectedItem).CategoriaId;

            DatosGenerales datosGeneralesNew = new DatosGenerales()
            {
                Categoria= (float)Convert.ToInt32(categoriasSel),
                Destino= (float)Convert.ToInt32(destinoSel),
                Peso= (float)Convert.ToInt32(txtPeso.Text)
                

            };
            PrediccionesIniciar prediccionesIniciar = new PrediccionesIniciar();
            prediccionesIniciar.Llenar(datosGeneralesNew);
            Prediccion prediccion = model.Predict(prediccionesIniciar.datosGenerales);
            txtResultado.Text = "El tiempo de entrega del paquete se estima en:";
            txtResultado1.Text= prediccion.TiempoEntrega.ToString() + " hs.";
            
        }
        private static void Evaluate(PredictionModel<DatosGenerales, Prediccion> model)
        {

            var testData = new Microsoft.ML.Data.TextLoader(testDataPath).CreateFrom<DatosGenerales>(useHeader: true, separator: ',');
            var evaluator = new Microsoft.ML.Models.RegressionEvaluator();

            RegressionMetrics regressionMetrics = evaluator.Evaluate(model, testData);
            Console.WriteLine($"R Model Score: {regressionMetrics.Rms}");
            Console.WriteLine($"R Squared: {regressionMetrics.RSquared}");


        }

   
    }
    public class Destino
    {
        public string DestinoItem { get; set; }
        public int DestinoId { get; set; }

    }
    public class Categorias
    {
        public string CategoriaItem { get; set; }
        public int CategoriaId { get; set; }

    }
}
