// ArcGISMapsSDK

using ArcGISMapsSDK.UX;
using ArcGISMapsSDK.UX.Security;

// .Net

using System.Collections.Generic;

// Unity

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ArcGISMapsSDK.Editor
{
	public class AuthConfigEditor
	{
		// Template and Style Path Names

		private const string AuthConfigRowTemplate = "AuthConfigRowTemplate";
		private const string AuthConfigTemplate = "AuthEditorTemplate";
		private const string AuthStyles = "AuthStyles";
		private const string AuthMappingRowTemplate = "AuthUrlRowTemplate";

		// API Key

		private const string APIKeyTextName = "api-key-text-name";

		// Config Names

		private const string ConfigTableName = "config-table-body";
		private const string AddConfigButtonName = "add-config-button";
		private const string RemoveConfigButtonName = "remove-config-button";
		private const string ActiveConfigButtonName = "auth-config-active-toggle";
		private const string AuthConfigNameField = "auth-config-name-field";
		private const string AuthClientIDField = "auth-client-id-field";
		private const string AuthRedirectUriField = "auth-redirect-uri-field";

		// Service URL names

		private const string MappingTableName = "resource-url-table-body";
		private const string AddMaapingButtonName = "add-resource-url-button";
		private const string RemoveMappingButtonName = "remove-resource-url-button";
		private const string ActiveMappingButtonName = "resource-url-active-toggle";
		private const string ServiceUrlFieldName = "resource-url-field-name";
		private const string ServiceUrlAssignConfigSwitchName = "resource-url-assign-config-switch";

		// Highlight Style Class Names

		private const string Normal = "auth-row-text-normal";
		private const string Highlight = "auth-row-text-highlight";

		// Private

		private List<string> configNames = new List<string>();
		private List<TemplateContainer> configRows = new List<TemplateContainer>();
		private List<TemplateContainer> mappingRows = new List<TemplateContainer>();

		private ArcGISMapController mapController;
		private VisualElement configTable;
		private VisualElement mappingTable;
		private VisualElement mapEditor;
		private VisualTreeAsset authEditorContent;
		private VisualTreeAsset configRowTemplate;
		private VisualTreeAsset mappingRowTemplate;

		private OAuthAuthenticationConfiguration selectedConfig;
		private OAuthAuthenticationConfigurationMapping selectedMapping;

		// Methods

		public AuthConfigEditor(VisualElement editor, ArcGISMapController controller)
		{
			mapEditor = editor;
			mapController = controller;

			var authConfigTemplatePath = MapControllerUtilities.FindAssetPath(AuthConfigTemplate);
			authEditorContent = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(authConfigTemplatePath);
			authEditorContent.CloneTree(mapEditor);

			configTable = mapEditor.Query<VisualElement>(name: ConfigTableName);
			mappingTable = mapEditor.Query<VisualElement>(name: MappingTableName);

			var stylePath = MapControllerUtilities.FindAssetPath(AuthStyles);
			mapEditor.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(stylePath));

			var configRowTemplatePath = MapControllerUtilities.FindAssetPath(AuthConfigRowTemplate);
			configRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(configRowTemplatePath);

			var mappingRowTemplatePath = MapControllerUtilities.FindAssetPath(AuthMappingRowTemplate);
			mappingRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(mappingRowTemplatePath);

			InitUIButtons();
			InitApiKey();
			CreateConfigurations();
			CreateConfigMappings();
			GetConfigNames();
		}

		private void InitUIButtons()
		{
			Button addConfigButton = mapEditor.Query<Button>(name: AddConfigButtonName);
			addConfigButton.RegisterCallback<MouseUpEvent>(evnt =>
			{
				DeselectConfigRows();
				mapController.Configurations.Add(new OAuthAuthenticationConfiguration() { Name = CreateConfigName() });
				CreateConfiguration(mapController.Configurations[mapController.Configurations.Count - 1]);
				GetConfigNames();
				MapControllerUtilities.MarkDirty(mapController);
			});

			Button removeConfigButton = mapEditor.Query<Button>(name: RemoveConfigButtonName);
			removeConfigButton.RegisterCallback<MouseUpEvent>(evnt =>
			{
				if (selectedConfig == null)
				{
					return;
				}

				for (int index = 0; index < mapController.Configurations.Count; index++)
				{
					if (mapController.Configurations[index] == selectedConfig)
					{
						mapController.Configurations.RemoveAt(index);
						configRows.Clear();
						configTable.Clear();
						CreateConfigurations();
						GetConfigNames();
						mappingRows.Clear();
						mappingTable.Clear();
						CreateConfigMappings();
						MapControllerUtilities.MarkDirty(mapController);
					}
				}
			});

			Button addUrlButton = mapEditor.Query<Button>(name: AddMaapingButtonName);
			addUrlButton.RegisterCallback<MouseUpEvent>(evnt =>
			{
				DeselectMappingRows();
				mapController.ConfigMappings.Add(new OAuthAuthenticationConfigurationMapping() { ServiceURL = string.Empty, Configuration = null });
				CreateConfigMapping(mapController.ConfigMappings[mapController.ConfigMappings.Count - 1]);
				MapControllerUtilities.MarkDirty(mapController);
			});

			Button removeUrlButton = mapEditor.Query<Button>(name: RemoveMappingButtonName);
			removeUrlButton.RegisterCallback<MouseUpEvent>(evnt =>
			{
				if (selectedMapping == null)
				{
					return;
				}

				for (int index = 0; index < mapController.ConfigMappings.Count; index++)
				{
					if (mapController.ConfigMappings[index] == selectedMapping)
					{
						mapController.ConfigMappings.RemoveAt(index);
						mappingRows.Clear();
						mappingTable.Clear();
						CreateConfigMappings();
						GetConfigNames();
						MapControllerUtilities.MarkDirty(mapController);
					}
				}
			});
		}

		private void InitApiKey()
		{
			TextField apiKeyField = mapEditor.Query<TextField>(name: APIKeyTextName);
			apiKeyField.value = mapController.APIKey;
			apiKeyField.RegisterValueChangedCallback(evnt =>
			{
				mapController.APIKey = evnt.newValue.Trim();
				apiKeyField.value = mapController.APIKey;
				MapControllerUtilities.MarkDirty(mapController);
			});
		}

		private string CreateConfigName()
		{
			int number = mapController.Configurations.Count + 1;
			const string defaultTitle = "Configuration ";
			var configs = mapController.Configurations;

			for (int index = 0; index < configs.Count; index++)
			{
				if (configs[index].Name == defaultTitle + number)
				{
					number++;
					index = 0;
				}
			}

			return defaultTitle + number;
		}

		private void CreateConfigurations()
		{
			selectedConfig = null;

			foreach (var config in mapController.Configurations)
			{
				CreateConfiguration(config);
			}
		}

		private void CreateConfiguration(OAuthAuthenticationConfiguration config)
		{
			TemplateContainer configRow = configRowTemplate.CloneTree();

			TextField configName = configRow.Query<TextField>(name: AuthConfigNameField);
			configName.value = config.Name;
			configName.RegisterValueChangedCallback(evnt =>
			{
				config.Name = evnt.newValue;
				UpdateMappingConfigNames(evnt.previousValue, evnt.newValue);
				MapControllerUtilities.MarkDirty(mapController);
			});

			Button activeButton = configRow.Query<Button>(name: ActiveConfigButtonName);
			MapControllerUtilities.ToggleCheckbox(activeButton, false);
			activeButton.RegisterCallback<MouseUpEvent>(envt =>
			{
				DeselectConfigRows();
				selectedConfig = config;
				configName.RemoveFromClassList(Normal);
				configName.AddToClassList(Highlight);
				MapControllerUtilities.ToggleCheckbox(activeButton, true);
				MapControllerUtilities.MarkDirty(mapController);
			});

			TextField clientID = configRow.Query<TextField>(name: AuthClientIDField);
			clientID.value = config.ClientID;
			if (config.ClientID == string.Empty)
			{
				clientID.value = "Enter Client ID";
			}
			clientID.RegisterValueChangedCallback(evnt =>
			{
				config.ClientID = evnt.newValue.Trim();
				clientID.value = config.ClientID;
				MapControllerUtilities.MarkDirty(mapController);
			});

			TextField redirectUriField = configRow.Query<TextField>(name: AuthRedirectUriField);
			redirectUriField.value = config.RedirectURI;
			if (config.RedirectURI == string.Empty)
			{
				redirectUriField.value = "Enter Redirect URI";
			}
			redirectUriField.RegisterValueChangedCallback(evnt =>
			{
				config.RedirectURI = evnt.newValue.Trim();
				redirectUriField.value = config.RedirectURI;
				MapControllerUtilities.MarkDirty(mapController);
			});

			configRows.Add(configRow);
			configTable.Add(configRow);
		}

		private void DeselectConfigRows()
		{
			selectedConfig = null;

			foreach (var configRow in configRows)
			{
				TextField configName = configRow.Query<TextField>(name: AuthConfigNameField);
				configName.RemoveFromClassList(Highlight);
				configName.AddToClassList(Normal);

				Button activeButton = configRow.Query<Button>(name: ActiveConfigButtonName);
				MapControllerUtilities.ToggleCheckbox(activeButton, false);
			}
		}

		private void CreateConfigMappings()
		{
			selectedMapping = null;

			foreach (var url in mapController.ConfigMappings)
			{
				CreateConfigMapping(url);
			}
		}

		private void CreateConfigMapping(OAuthAuthenticationConfigurationMapping mapping)
		{
			TemplateContainer mappingRow = mappingRowTemplate.CloneTree();

			TextField urlField = mappingRow.Query<TextField>(name: ServiceUrlFieldName);
			urlField.value = mapping.ServiceURL;
			if (mapping.ServiceURL == string.Empty)
			{
				urlField.value = "Enter Service URL";
			}
			urlField.RegisterValueChangedCallback(evnt =>
			{
				mapping.ServiceURL = evnt.newValue.Trim();
				urlField.value = mapping.ServiceURL;
				MapControllerUtilities.MarkDirty(mapController);
			});

			Button activeButton = mappingRow.Query<Button>(name: ActiveMappingButtonName);
			MapControllerUtilities.ToggleCheckbox(activeButton, false);
			activeButton.RegisterCallback<MouseUpEvent>(envt =>
			{
				DeselectMappingRows();
				selectedMapping = mapping;
				urlField.AddToClassList(Highlight);
				MapControllerUtilities.ToggleCheckbox(activeButton, true);
				MapControllerUtilities.MarkDirty(mapController);
			});

			VisualElement configWrapper = mappingRow.Query<VisualElement>(name: ServiceUrlAssignConfigSwitchName);
			var configDropdown = new PopupField<string>("", GetConfigNames(), 0);
			configDropdown.value = GetConfigNameValue(mapping.Configuration);
			configWrapper.Add(configDropdown);
			SetConfig(configDropdown.value, mapping);
			configWrapper.RegisterCallback<ChangeEvent<string>>(evnt =>
			{
				SetConfig(evnt.newValue, mapping);
				MapControllerUtilities.MarkDirty(mapController);
			});

			mappingRows.Add(mappingRow);
			mappingTable.Add(mappingRow);
		}

		private void DeselectMappingRows()
		{
			selectedMapping = null;

			foreach (var mappingRow in mappingRows)
			{
				TextField urlField = mappingRow.Query<TextField>(name: ServiceUrlFieldName);
				urlField.RemoveFromClassList(Highlight);

				Button activeButton = mappingRow.Query<Button>(name: ActiveMappingButtonName);
				MapControllerUtilities.ToggleCheckbox(activeButton, false);
			}
		}

		private List<string> GetConfigNames()
		{
			configNames.Clear();
			configNames.Add("None");
			foreach (var config in mapController.Configurations)
			{
				configNames.Add(config.Name);
			}
			return configNames;
		}

		private string GetConfigNameValue(OAuthAuthenticationConfiguration config)
		{
			var none = configNames[0];

			if (config == null)
			{
				return none;
			}

			foreach (var configName in configNames)
			{
				if (configName == config.Name)
				{
					return configName;
				}
			}
			return none;
		}

		private void SetConfig(string name, OAuthAuthenticationConfigurationMapping url)
		{
			foreach (var config in mapController.Configurations)
			{
				if (config.Name == name)
				{
					url.Configuration = config;
					return;
				}
			}
		}

		private void UpdateMappingConfigNames(string previousName, string newName)
		{
			for (int index = 0; index < configNames.Count; index++)
			{
				if (configNames[index] == previousName)
				{
					configNames[index] = newName;
				}
			}

			foreach (var row in mappingRows)
			{
				VisualElement element = row.Query<VisualElement>(name: ServiceUrlAssignConfigSwitchName);
				PopupField<string> dropdown = element.Query<PopupField<string>>();
				if (dropdown.value == previousName)
				{
					dropdown.value = newName;
				}
			}
		}
	}
}
