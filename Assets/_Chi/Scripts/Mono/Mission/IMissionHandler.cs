using _Chi.Scripts.Mono.Mission.Events;

namespace _Chi.Scripts.Mono.Mission
{
    public interface IMissionHandler
    {
        void OnStart(MissionEvent ev);

        void OnStop();
    }
}