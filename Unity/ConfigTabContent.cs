using AdvancedCompany.Config;
using AdvancedCompany.Unity.Moons;
using UnityEngine;

namespace AdvancedCompany
{
    public class ConfigTabContent : MonoBehaviour
    {
        public Transform Container;
        public GameObject ContainerPrefab;
        public GameObject ItemContainerPrefab;
        public GameObject UnlockableContainerPrefab;
        public GameObject PerkContainerPrefab;
        public GameObject WeatherContainerPrefab;
        public GameObject MoonContainerPrefab;
        public GameObject ScrapContainerPrefab;
        public GameObject EnemyContainerPrefab;

        public ConfigContainer AddContainer(string name, string description)
        {
            var container = GameObject.Instantiate(ContainerPrefab, Container);
            var c = container.GetComponent<ConfigContainer>();
            c.Initialize(name, description);
            return c;
        }

        public ConfigContainer AddItemContainer(string name, string description)
        {
            var container = GameObject.Instantiate(ItemContainerPrefab, Container);
            var c = container.GetComponent<ConfigContainer>();
            c.Initialize(name, description);
            return c;
        }

        public ConfigContainer AddUnlockableContainer(string name, string description)
        {
            var container = GameObject.Instantiate(UnlockableContainerPrefab, Container);
            var c = container.GetComponent<ConfigContainer>();
            c.Initialize(name, description);
            return c;
        }

        public ConfigContainer AddScrapContainer(string name, string description)
        {
            var container = GameObject.Instantiate(ScrapContainerPrefab, Container);
            var c = container.GetComponent<ConfigContainer>();
            c.Initialize(name, description);
            return c;
        }

        public ConfigContainer AddEnemyContainer(string name, string description)
        {
            var container = GameObject.Instantiate(EnemyContainerPrefab, Container);
            var c = container.GetComponent<ConfigContainer>();
            c.Initialize(name, description);
            return c;
        }

        public ConfigContainer AddPerkContainer(string name, string description)
        {
            var container = GameObject.Instantiate(PerkContainerPrefab, Container);
            var c = container.GetComponent<ConfigContainer>();
            c.Initialize(name, description);
            return c;
        }

        internal MoonsContainer AddMoonContainer(string name, string description, LobbyConfiguration.MoonsConfig config)
        {
            var container = GameObject.Instantiate(MoonContainerPrefab, Container);
            var c = container.GetComponent<MoonsContainer>();
            foreach (var kv in LobbyConfiguration.AllMoonsConfig.Moons)
            {
                c.AddMoon(kv.Key);
            }
            foreach (var kv in LobbyConfiguration.AllScrapConfig.Items)
            {
                c.AddScrap(kv.Key);
            }
            foreach (var kv in LobbyConfiguration.AllEnemiesConfig.Enemies)
            {
                c.AddEnemy(kv.Key);
            }
            c.Initialize(name, description);
            c.SetValue(config);
            return c;
        }

        public ConfigContainer AddWeatherContainer(string name, string description)
        {
            var container = GameObject.Instantiate(WeatherContainerPrefab, Container);
            var c = container.GetComponent<ConfigContainer>();
            c.Initialize(name, description);
            return c;
        }
    }
}
