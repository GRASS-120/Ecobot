#define DEBUG_REPAINT


#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
#endif

namespace Sisus.Shared.EditorOnly
{
	public enum InspectorType
	{
		InspectorWindow,
		InspectorWindowDebugMode,
		PropertiesWindow
	}
}