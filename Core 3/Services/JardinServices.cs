using MongoDB.Driver;
using Core_3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core_3.Services
{
    public class JardinServices
    {
        private IMongoCollection<Plantas> _plantas;
        private IMongoCollection<Usuario> _usuario;

        public JardinServices (IDBSettings settings)
        {
            //vamos al servidor por el service
            var client = new MongoClient(settings.Server);
            //vamos por la base de datos 
            var database = client.GetDatabase(settings.Database);
            //aca agarramos la base de dato especifica de plantas
            _plantas = database.GetCollection<Plantas>(settings.Collection);
            var databaseUser = client.GetDatabase("Usuarios");
            _usuario = databaseUser.GetCollection<Usuario>("usuarios");
        }

        //trae todas las plantas de la base de datos de mongoDB
        public List<Plantas> Get()
        {
            //me retorna todas las plantas
            return _plantas.Find(d => d.fechadeEliminacion == null).ToList();
        }
        //crea una nueva planta en la base de datos de mongoDB
        public Plantas Create(Plantas planta,string idAutor)
        {
            var autor = _usuario.Find(d => d.Id == idAutor).FirstOrDefault();

            if (autor == null)
            {
                throw new InvalidOperationException("Author not found.");
            }

            planta.fechaCreacion = DateTime.Now;
            planta.autor = new Usuario { Id = autor.Id,UrlImage = autor.UrlImage, Email = autor.Email,Username = autor.Username };

            _plantas.InsertOne(planta);
            return planta;
        }
        public void Update(string id,Plantas planta)
        {
            _plantas.ReplaceOne(p => p.Id == id, planta);
        }
        public Plantas GetById(string idPlanta)
        {
            return _plantas.Find(d => d.Id == idPlanta).First();
        }

        public void Delete(string id)
        {

            var planta = _plantas.Find(d => d.Id == id).First();
            planta.fechadeEliminacion = DateTime.Now;

            _plantas.ReplaceOne(p => p.Id == id, planta);
        }

    }
}
