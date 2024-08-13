using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core_3.Models
{
    public class Plantas
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("color")]
        public string? Color { get; set; }

        [BsonElement("imagenesUint8List")]
        public List<byte[]>? ImagenesUint8List { get; set; }

        [BsonElement("descripcion")]
        public string Descripcion { get; set; }

        [BsonElement("horasAluzSolar")]
        public string? HorasAluzSolar { get; set; }


        [BsonElement("diasDeRegarMinimo")]
        public int? DiasDeRegarMinimo { get; set; }


        [BsonElement("diasDeRegarMaximo")]
        public int? DiasDeRegarMaximo { get; set; }

        [BsonElement("ultimoRiego")]
        public DateTime? UltimoRiego { get; set; }


        [BsonElement("autor")]
        public Usuario? autor { get; set; }

        [BsonElement("fechaCreacion")]
        public DateTime? fechaCreacion{ get; set; }

        [BsonElement("fechadeEliminacion")]
        public DateTime? fechadeEliminacion { get; set; }

    }
    public class CreatePlantaDto
    {
        public string Name { get; set; }
        public string? Color { get; set; }
        public string Descripcion { get; set; }
        public string? HorasAluzSolar { get; set; }
        public int? DiasDeRegarMinimo { get; set; }
        public int? DiasDeRegarMaximo { get; set; }
        public DateTime? UltimoRiego { get; set; }
        public List<IFormFile>? Imagenes { get; set; }
        public DateTime? fechaCreacion { get; set; }
        public DateTime? fechaDeEliminacion { get; set; }
    }
}
