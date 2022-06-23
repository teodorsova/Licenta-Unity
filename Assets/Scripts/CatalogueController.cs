using Proyecto26;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts;
using UnityEngine.Networking;
using UnityEditor;
using RSG;
using Proyecto26.Helper;

public class CatalogueController : MonoBehaviour
{
    private GameObject myContainer;
    private GameObject hudCanvas;
    private GameObject catalogueContainer;
    private GameObject prefabButton;
    private GameObject categoryButton;
    private GameObject contentContainer;
    private GameObject buttonContent;
    private GameObject textMeshProStatus;
    private Dictionary<string, GameObject> contentPerRoom = new Dictionary<string, GameObject>();
    private Button catalogueButton;
    private ScrollRect scrollRect;
    private List<ButtonPrefabPair> buttonPrefabPairs = new List<ButtonPrefabPair>();
    private List<GameObject> instantiatedPrefabs = new List<GameObject>();
    private List<GameObject> instantiatedPrefabsForButtons = new List<GameObject>();
    private RectTransform viewport;

    private List<XmlNodeList> furnitureData = new List<XmlNodeList>();
    private XmlDocument doc = new XmlDocument();

    private Vector3 prefabPos = new Vector3(-300, -40, 10);
    private Vector3 categoryButtonPos = new Vector3(200, -100, 0);

    public Text label;
    public byte[] configFile;
    public string assetBundleName = "teofurnituresbinder";
    public string configFileName = "config.xml";
    private AssetBundle assetBundle;

    private Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();

    private Dictionary<string, List<GameObject>> prefabDictionary = new Dictionary<string, List<GameObject>>();

    [DllImport("__Internal")]
    private static extern void FurnitureSpawn(string Name, string price, string companyName);


    void Start()
    {
        AssetBundle.UnloadAllAssetBundles(true);
        InitComponents();
        RestClient.Get("http://localhost:5220/api/azure/get/config").Then(response =>
        {
            GetConfigBlob(response);
        });
    }
 
    private void InitComponents()
    {
        myContainer = GameObject.Find("Container");
        hudCanvas = GameObject.Find("HUDCanvas");
        catalogueContainer = hudCanvas.transform.Find("CatalogueContainer").gameObject;
        catalogueButton = hudCanvas.transform.Find("Options").GetComponent<Button>();
        catalogueButton.onClick.AddListener(delegate { OnClickOptions(); });
        scrollRect = catalogueContainer.transform.Find("Catalogue").GetComponent<ScrollRect>();
        viewport = scrollRect.transform.Find("Viewport").GetComponent<RectTransform>();
        prefabButton = Resources.Load<GameObject>("Button/Button");
        categoryButton = Resources.Load<GameObject>("Button/CategoryButton");
        contentContainer = Resources.Load<GameObject>("Container/Content");
        textMeshProStatus = hudCanvas.transform.Find("Status").gameObject;

        InitRoomButtons();

        buttonPrefabPairs.Clear();
        catalogueContainer.transform
            .Find("Catalogue")
            .GetComponent<ScrollRect>().content = buttonContent.GetComponent<RectTransform>();



        foreach (string room in Room.roomNames)
        {
            prefabDictionary.Add(room, new List<GameObject>());
            var content = Instantiate(contentContainer, viewport);
            content.name = room + "Content";
            content.SetActive(false);
            contentPerRoom.Add(room, content);
            AddBackButton(content, buttonContent);
        }
    }

    private void GetConfigBlob(ResponseHelper response)
    {
        try
        {
            doc.LoadXml(response.Text);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        XmlNodeList nodeList = doc.ChildNodes;
        XmlNodeList companiesList = nodeList[0].ChildNodes;
        XmlNodeList companyNamesList = companiesList[0].ChildNodes;

        foreach (XmlNode companyName in companyNamesList)
        {
            XmlNodeList fileNameList = companyName.ChildNodes;
            foreach (XmlNode fileName in fileNameList)
            {
                foreach (XmlNode furniture in fileName.ChildNodes)
                {
                    XmlNodeList furnitureDetails = furniture.ChildNodes;
                    furnitureData.Add(furnitureDetails);
                }
                ExecuteOnMainThread.RunOnMainThread.Enqueue(() => {
                    RestClient.Post(new RequestHelper
                    {
                        Uri = "http://localhost:5220/api/azure/get/package",
                        BodyString = "{\"FileName\":" + $"\"{fileName.Name}\"" + "}",
                        ProgressCallback = progress => textMeshProStatus.GetComponent<TextMeshPro>().text = "Downloading " + fileName.Name + "... " + progress * 100 + "%"
                    }).Then(response =>
                    {
                        //Debug.Log(response.Text);
                        //EditorUtility.DisplayDialog("Response", response.Text, "Ok");
                        LoadAssets(AssetBundle.LoadFromMemory(response.Data));
                        textMeshProStatus.GetComponent<TextMeshPro>().text = "Done";

                    }).Then(() =>
                    {
                        LoadPrefabs();
                    }).Catch(error =>
                    {
                        Debug.LogError(error.Message);

                    });
                });
                //coroutineQueue.Enqueue(AzureClient.blobService.GetAssetBundle(GetAssetBundleComplete, AzureClient.container + "/" + fileName.Name));
                

            }
        }
        if (!ExecuteOnMainThread.RunOnMainThread.IsEmpty)
        {
            Action action;
            while (ExecuteOnMainThread.RunOnMainThread.TryDequeue(out action))
            {
                if (action != null)
                {
                    action.Invoke();
                    action = null;
                }
            }
        }
    }

    private IEnumerator StartCoroutineCoordinator()
    {
        yield return StartCoroutine(CoroutineCoordinator());
    }

    private IEnumerator CoroutineCoordinator()
    {
        while (true)
        {
            while (coroutineQueue.Count > 0)
                yield return StartCoroutine(coroutineQueue.Dequeue());

            yield return null;
        }
    }
    private void LoadAssets(AssetBundle bundle)
    {
        Debug.Log(bundle.name);
        GameObject[] loadedObjects = null;
        try
        {
            // Load the object asynchronously
            loadedObjects = bundle.LoadAllAssets<GameObject>();
            XmlNodeList nodeList = doc.ChildNodes;
            XmlNodeList companiesList = nodeList[0].ChildNodes;
            XmlNodeList companyNamesList = companiesList[0].ChildNodes;

            foreach (GameObject obj in loadedObjects)
            {
                foreach (XmlNode companyName in companyNamesList)
                {
                    XmlNodeList fileNameList = companyName.ChildNodes;
                    foreach (XmlNode fileName in fileNameList)
                    {
                        foreach (XmlNode furniture in fileName.ChildNodes)
                        {
                            XmlNodeList furnitureDetails = furniture.ChildNodes;
                            try
                            {
                                string bundleFileName = fileName.Name;
                                int fileExtPos = bundleFileName.LastIndexOf(".");
                                if (fileExtPos >= 0)
                                    bundleFileName = bundleFileName.Substring(0, fileExtPos);
                                if (furnitureDetails[0].InnerText.Equals(obj.name) && (bundle.name.Equals(fileName.Name) || bundle.name.Equals(bundleFileName)))
                                {
                                    prefabDictionary[furnitureDetails[2].InnerText].Add(obj);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.Log(ex.InnerException);
                            }
                        }

                    }
                }
            }
        }
        catch (Exception ex)
        {

        }


        foreach (GameObject obj in loadedObjects)
        {
            AddPrefab(obj, new Vector3(0, 4, 0));
        }
    }

    private void AddPrefab(GameObject obj, Vector3 position = default(Vector3))
    {
        AddPrefab(position, Quaternion.identity, Vector3.one, Color.clear, obj);
    }

    private void AddPrefab(Vector3 position, Quaternion rotation, Vector3 scale, Color color, GameObject obj)
    {
        if (assetBundle == null)
        {
            return;
        }
    }

    private void InitRoomButtons()
    {

        buttonContent = Instantiate(contentContainer, viewport);
        buttonContent.name = "MainMenuButtons";

        foreach (string room in Room.roomNames)
        {
            var categoryButtonInstance = Instantiate(categoryButton);

            categoryButtonInstance.transform.SetParent(buttonContent.transform);
            categoryButtonInstance.transform.localScale = new Vector3(1, 1, 1);
            categoryButtonInstance.transform.localPosition = categoryButtonPos;
            categoryButtonPos += new Vector3(400, 0, 0);
            categoryButtonInstance.GetComponentInChildren<TextMeshProUGUI>().text = room;
            categoryButtonInstance.GetComponent<Button>().onClick.AddListener(delegate { OnClickCategoryButton(room); });
        }

    }

    private void LoadPrefabs()
    {


        foreach (KeyValuePair<string, List<GameObject>> prefabArray in prefabDictionary)
        {


            foreach (GameObject prefab in prefabArray.Value)
            {
                if (!instantiatedPrefabsForButtons.Find(p => p == prefab))
                {
                    GameObject catalogueButtonWithFurniture = CreateButtonWithFurniture(prefab);
                    prefabPos += new Vector3(40, 0, 0);
                    catalogueButtonWithFurniture.transform.SetParent(contentPerRoom[prefabArray.Key].transform);
                    catalogueButtonWithFurniture.transform.localScale = new Vector3(1.1f, 1.1f, 1);
                    catalogueButtonWithFurniture.transform.localPosition = prefabPos;
                    catalogueButtonWithFurniture.transform.localRotation = Quaternion.identity;
                    catalogueButtonWithFurniture.GetComponent<Button>().onClick.AddListener(delegate
                    {
                        OnFurnitureButtonClick(catalogueButtonWithFurniture);
                    }
                    );
                    createButtonPrefabPair(prefab, catalogueButtonWithFurniture);
                }
            }
            //contentPerRoom[prefabArray.Key].SetActive(false);
        }

    }

    private void createButtonPrefabPair(GameObject prefab, GameObject catalogueButtonWithFurniture)
    {
        foreach (XmlNodeList furnitureDataList in furnitureData)
        {
            if (prefab.name == (furnitureDataList[0].InnerText))
            {
                ButtonPrefabPair pair = new ButtonPrefabPair(
                    catalogueButtonWithFurniture,
                    prefab,
                    furnitureDataList[1].InnerText,
                    furnitureDataList[3].InnerText
                    );
                buttonPrefabPairs.Add(pair);
                break;
            }
        }
    }

    private GameObject CreateButtonWithFurniture(GameObject prefab)
    {
        GameObject imageContainer = new GameObject();
        Image prefabImage = imageContainer.AddComponent<Image>();
        var furnitureTexture = RuntimePreviewGenerator.GenerateModelPreview(
            prefab.transform,
            140,
            140,
            false,
            true
            );

        var catalogueButtonWithFurniture = Instantiate(prefabButton) as GameObject;
        instantiatedPrefabsForButtons.Add(prefab);

        catalogueButtonWithFurniture.GetComponent<Image>().sprite = Sprite.Create(
            furnitureTexture,
            new Rect(0, 0,
            furnitureTexture.width,
            furnitureTexture.height),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit: 100f
            );
        return catalogueButtonWithFurniture;
    }



    private void AddBackButton(GameObject content, GameObject buttonContent)
    {
        var backButton = Instantiate(categoryButton);
        backButton.transform.SetParent(content.transform);
        backButton.transform.localScale = new Vector3(1, 1, 1);
        backButton.transform.localPosition = new Vector3(200, -100, 0);
        backButton.GetComponentInChildren<TextMeshProUGUI>().text = "Back";
        backButton.GetComponent<Button>().onClick.AddListener(delegate { OnClickBackButton(buttonContent); });
    }

    private void OnFurnitureButtonClick(GameObject button)
    {
        ButtonPrefabPair buttonPrefabPair = buttonPrefabPairs.Find((pair) => pair.Button == button);
        GameObject prefab = Instantiate(buttonPrefabPair.Prefab);
        prefab.transform.SetParent(myContainer.transform);
        prefab.name = buttonPrefabPair.Prefab.name;

        prefab.AddComponent<FurnitureMoveSCript>();
        try
        {
            prefab.GetComponent<BoxCollider>().enabled = true;
        }
        catch (Exception)
        {
            prefab.AddComponent<BoxCollider>();
        }

        try
        {
            Destroy(prefab.GetComponent<Rigidbody>());
        }
        catch (Exception)
        {

        }

        prefab.transform.localScale = new Vector3(4f, 4f, 4f);
        prefab.transform.position = new Vector3(0f, 2f, 0f);
        instantiatedPrefabs.Insert(0, prefab);
        //If running in browser, sned instance to react in order to show it in listview
        AddToBrowserList(buttonPrefabPair);
    }

    private void AddToBrowserList(ButtonPrefabPair buttonPrefabPair)
    {

#if UNITY_WEBGL == true && UNITY_EDITOR == false
       if (buttonPrefabPair.Price != "0.0" && buttonPrefabPair.CompanyName != "")
        {
            FurnitureSpawn(buttonPrefabPair.Prefab.name, buttonPrefabPair.Price, buttonPrefabPair.CompanyName);
        } else
        {
            FurnitureSpawn(buttonPrefabPair.Prefab.name, "0.0", "");
        }
#endif
    }

    public void DeleteFurniture(string name)
    {
        try
        {
            GameObject prefab = instantiatedPrefabs.Find((prefab) => prefab.gameObject.name == name);
            instantiatedPrefabs.Remove(prefab);
            Destroy(prefab);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }


    private void OnClickCategoryButton(string category)
    {
        foreach (Transform child in viewport.transform)
        {
            child.gameObject.SetActive(false);
        }
        Debug.Log(category);

        var content = viewport.transform.Find(category + "Content").GetComponent<RectTransform>();
        content.gameObject.SetActive(true);
        catalogueContainer.transform.Find("Catalogue").GetComponent<ScrollRect>().content = content;

    }

    private void OnClickBackButton(GameObject content)
    {
        foreach (Transform child in viewport.transform)
        {
            child.gameObject.SetActive(false);
        }

        content.gameObject.SetActive(true);
        catalogueContainer.transform.Find("Catalogue").GetComponent<ScrollRect>().content = content.GetComponent<RectTransform>();

    }

    private void OnClickOptions()
    {
        if (catalogueContainer.gameObject.activeSelf == false)
        {
            catalogueContainer.gameObject.SetActive(true);
        }
        else
        {
            catalogueContainer.gameObject.SetActive(false);
        }
    }
}
