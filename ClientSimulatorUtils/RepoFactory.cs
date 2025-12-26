using ClientSimulator_BL.Interfaces;
using ClientSimulator_DL;
using ClientSimulator_DL.Repository;

namespace ClientSimulatorUtils
{
    public static class RepoFactory
    {
        public static IPersoonRepository GeefPersoonRepository()
        {
            return new PersoonRepository();
        }
        public static IAchternaamRepository GeefAchternaamRepository()
        {
            return new AchternaamRepository();
        }
    }
}
