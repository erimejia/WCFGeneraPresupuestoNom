using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    public class clsUsuarioAD
    {
        public string IdUsuario { get; set; }
        public string Correo { get; set; }
        public string NombreCompleto { get; set; }
        public string Iniciales { get; set; }
        public bool EstaBloqueado { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Titulo { get; set; }
        public string TipoUsuario { get; set; }
        public string Departamento { get; set; }
        public string Work { get; set; }
        public string Office { get; set; }
        public Collection<string> grupos { get; set; }
        public Collection<string> listaOus { get; set; }


        public enum PerfilesSGA
        {
            CIMA = 0,
            DECANO = 1,
            JEFEDEPARTAMENTO = 2,
            PROFESOR = 3,
            NINGUNO = 4
        };
        internal PerfilesSGA perfilActual()
        {
            PerfilesSGA perfil = PerfilesSGA.NINGUNO;
            if (grupos.Count > 0)
            {

                string[] perfiles = { "CIMA", "DECANO", "JEFE DE DEPARTAMENTO", "PROFESOR" };
                perfil = PerfilesSGA.NINGUNO;
                for (int i = 0; i < 4; i++)
                {
                    IEnumerator<string> enumGrupos = grupos.GetEnumerator();
                    while (enumGrupos.MoveNext())
                    {
                        string cad = enumGrupos.Current;
                        if (cad.ToUpper().Equals(perfiles[i]))
                        {
                            perfil = (PerfilesSGA)i;
                            break;
                        }
                    }
                    if (perfil != PerfilesSGA.NINGUNO)
                        break;
                }
            }
            return perfil;
        }

        public clsUsuarioAD()
        {
            listaOus = new Collection<string>();
            grupos = new Collection<string>();
            this.Apellidos = string.Empty;
            this.Correo = string.Empty;
            this.Departamento = string.Empty;
            this.IdUsuario = string.Empty;
            this.Iniciales = string.Empty;
            this.Nombre = string.Empty;
            this.NombreCompleto = string.Empty;
            this.TipoUsuario = string.Empty;
            this.Titulo = string.Empty;
            this.Work = string.Empty;
            this.Office = string.Empty;
        }
    }
}
