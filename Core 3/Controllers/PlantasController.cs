using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Authorization;
using Core_3.Services;
using Core_3.Models;

namespace Core_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlantasController : ControllerBase
    {
        public JardinServices _jardinService;

        public PlantasController(JardinServices jardinService)
        {
            //constructor para que ya venga con cada propiedad igualada
            _jardinService = jardinService;
        }

        //peticion para que me devuelva todas las plantas de la base de datos 
        [HttpGet]
        public ActionResult<List<Plantas>> Get()
        {
            //me retorna toda la lista de la base de datos
            return _jardinService.Get();
        }

        //peticion para que me devuelva todas las plantas de la base de datos 
        [HttpGet("{id}")]
        public ActionResult<Plantas> GetById(String id)
        {
            //me retorna toda la lista de la base de datos
            return _jardinService.GetById(id);

        }

        //metodo para agregar una nueva planta
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreatePlantaDto createPlantaDto, [FromQuery] string idAutor)
        {
            var planta = new Plantas
            {
                Name = createPlantaDto.Name,
                Color = createPlantaDto.Color,
                Descripcion = createPlantaDto.Descripcion,
                HorasAluzSolar = createPlantaDto.HorasAluzSolar,
                DiasDeRegarMinimo = createPlantaDto.DiasDeRegarMinimo,
                DiasDeRegarMaximo = createPlantaDto.DiasDeRegarMaximo,
                UltimoRiego = createPlantaDto.UltimoRiego,
                ImagenesUint8List = new List<byte[]>(),
                fechadeEliminacion = createPlantaDto.fechaDeEliminacion,
                fechaCreacion = createPlantaDto.fechaCreacion,
            };

            if (createPlantaDto.Imagenes != null)
            {
                foreach (var imagen in createPlantaDto.Imagenes)
                {
                    using var memoryStream = new MemoryStream();
                    await imagen.CopyToAsync(memoryStream);
                    planta.ImagenesUint8List.Add(memoryStream.ToArray());
                }
            }
            var newPlant = _jardinService.Create(planta, idAutor);
            planta.autor = newPlant.autor;
            return Ok(planta);
        }

        [HttpPut]
        public ActionResult Update([FromBody] Plantas planta)
        {
            _jardinService.Update(planta.Id, planta);
            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(string id)
        {
            _jardinService.Delete(id);
            return Ok();
        }
        
    }
}
