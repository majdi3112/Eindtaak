using ClientSimulator_BL.Model;
using ClientSimulator_DL.Repository;
using System.Collections.Generic;

namespace ClientSimulatorUtils.Services
{
    public class LandService
    {
        public LandService()
        {
            // Service kan data ophalen via managers indien nodig
            // Voorlopig gebruiken we direct repository voor eenvoud
        }

        public List<Land> GetAllLanden()
        {
            var repo = new LandRepository();
            return repo.GetAll();
        }

        public int GetOrCreateLandId(string landNaam)
        {
            var repo = new LandRepository();
            return repo.InsertOfOphalen(landNaam);
        }
    }
}
