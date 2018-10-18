using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANVI6MLNetCoreTechint
{
    public class PrediccionesIniciar
    {
        public ANVI6MLNetCoreTechint.MainPage.DatosGenerales datosGenerales = new MainPage.DatosGenerales();
        public void Llenar(MainPage.DatosGenerales datosGeneralesReceived)
        {
            datosGenerales = datosGeneralesReceived;
        }
    }
}
