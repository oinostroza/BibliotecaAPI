using MiProyecto.Application.Avisos.Queries.GetAvisosExcel;
using MiProyecto.Application.Common.Models;

namespace MiProyecto.Application.Common.Interfaces;

public interface IExcelService
{
    byte[]  GenerarArchivoUnico(List<AvisoExcelDto> data, string nombreMes);
}
