using Azure.StorageServices;
using RESTClient;
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

public class CatalogueController : MonoBehaviour
{
    private GameObject myContainer;
    private GameObject hudCanvas;
    private GameObject catalogueContainer;
    private GameObject prefabButton;
    private GameObject categoryButton;
    private GameObject contentContainer;
    private GameObject buttonContent;
    private Button catalogueButton;
    private ScrollRect scrollRect;
    private List<ButtonPrefabPair> buttonPrefabPairs = new List<ButtonPrefabPair>();
    private List<GameObject> instantiatedPrefabs = new List<GameObject>();
    private RectTransform viewport;

    private List<XmlNodeList> furnitureData = new List<XmlNodeList>();
    private XmlDocument doc;

    private Vector3 prefabPos = new Vector3(-300, -40, 10);
    private Vector3 categoryButtonPos = new Vector3(200, -100, 0);

    private bool isLoaded = false;
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

        InitComponents();
        AddLoadingButton();

        //StartCoroutine(AccessAzure());

        //var headers = wr.GetRequestHeader;
        
    }

    string signWithAccountKey(string stringToSign, string accountKey)
    {
        var hmacsha = new System.Security.Cryptography.HMACSHA256();
        hmacsha.Key = Convert.FromBase64String(accountKey);
        var signature = hmacsha.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
        return Convert.ToBase64String(signature);
    }

    private IEnumerator AccessAzure()
    {
        AuthorizationHeaders headers = new AuthorizationHeaders(
            AzureClient.client,
            Method.GET,
            $"https://{AzureClient.storageAccount}.blob.core.windows.net/{AzureClient.container}/{configFileName}",
            null,
            null,
            0);
        UnityWebRequest wr = new UnityWebRequest();
        wr.downloadHandler = new DownloadHandlerBuffer();
        wr.url = $"https://{AzureClient.storageAccount}.blob.core.windows.net/{AzureClient.container}/{configFileName}";
        wr.method = UnityWebRequest.kHttpVerbGET;
        wr.SetRequestHeader("accept", "*/*");
        wr.SetRequestHeader("accept-encoding", "gzip, deflate, br");
        wr.SetRequestHeader("accept-language", "en-US,en;q=0.9");
        wr.SetRequestHeader("authorization", $"SharedKey{AzureClient.storageAccount}:{signWithAccountKey("", AzureClient.accessKey)}");
        wr.SetRequestHeader("connection", "keep-alive");
        wr.SetRequestHeader("host", "");
        wr.SetRequestHeader("host", $"https://{AzureClient.storageAccount}.blob.core.windows.net/");
        
        //wr.SetRequestHeader("origin", "http://localhost:3000");
        //wr.SetRequestHeader("referer", "http://localhost:3000");
        wr.SetRequestHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.63 Safari/537.36");
        wr.SetRequestHeader("x-ms-version", "2022-06-06");
        wr.SetRequestHeader("x-ms-date", DateTime.Now.ToString());
        yield return wr.SendWebRequest();

        if (wr.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(wr.error);
            Debug.Log(wr.result);
        }
        else
        {
            // Show results as text
            Debug.Log(wr.downloadHandler.text);

            // Or retrieve results as binary data
            byte[] results = wr.downloadHandler.data;
        }
    }
    private void Update()
    {
        if (!isLoaded)
        {
            TappedLoadAssetBundle();
            isLoaded = true;
        }
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

        foreach(string room in Room.roomNames)
        {
            prefabDictionary.Add(room, new List<GameObject>());
        }
    }

    public void AddLoadingButton()
    {
        var categoryButtonInstance = Instantiate(categoryButton);
        categoryButtonInstance.transform.SetParent(viewport.transform);
        categoryButtonInstance.name = "loadingBtn";
        categoryButtonInstance.transform.localScale = new Vector3(1, 1, 1);
        categoryButtonInstance.transform.localPosition = new Vector3(0,0,0);
        categoryButtonInstance.GetComponentInChildren<TextMeshProUGUI>().text = "Loading...";
    }

    public IEnumerator RemoveLoadingButton()
    {  
        viewport.transform.Find("loadingBtn").gameObject.transform.SetParent(null);
        yield return null;
    }

    public void TappedLoadAssetBundle()
    {
        string filename = assetBundleName + ".unity3d";
        string resourcePath = AzureClient.container + "/" + configFileName;
        StartCoroutine(AzureClient.blobService.GetXmlBlob<XmlDocument>(GetConfigBlob, resourcePath));   
    }

    private void GetConfigBlob(IRestResponse<XmlDocument> response)
    {
        doc = response.Data;
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
                coroutineQueue.Enqueue(AzureClient.blobService.GetAssetBundle(GetAssetBundleComplete, AzureClient.container + "/" + fileName.Name));

            }
        }
        coroutineQueue.Enqueue(InitRoomButtons());
        coroutineQueue.Enqueue(RemoveLoadingButton());
        coroutineQueue.Enqueue(LoadPrefabs());
        StartCoroutine(StartCoroutineCoordinator());
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
    private void GetAssetBundleComplete(IRestResponse<AssetBundle> response)
    {
            assetBundle = response.Data;
            StartCoroutine(LoadAssets(assetBundle));
    }

    private IEnumerator LoadAssets(AssetBundle bundle)
    {
        GameObject[] loadedObjects = null ;
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

        yield return loadedObjects;

        foreach (GameObject obj in loadedObjects)
        {
            AddPrefab(obj, new Vector3(0, 4, 0));
        }
    }

    private void AddPrefab(GameObject obj,Vector3 position = default(Vector3))
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

    private IEnumerator InitRoomButtons()
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

        yield return null;
    }

    private IEnumerator LoadPrefabs()
    {
        buttonPrefabPairs.Clear();
        catalogueContainer.transform
            .Find("Catalogue")
            .GetComponent<ScrollRect>().content = buttonContent.GetComponent<RectTransform>();

        foreach (KeyValuePair<string, List<GameObject>> prefabArray in prefabDictionary)
        {
            var content = Instantiate(contentContainer, viewport);
            content.name = prefabArray.Key + "Content";
            AddBackButton(content, buttonContent);
            foreach (GameObject prefab in prefabArray.Value)
            {
                GameObject catalogueButtonWithFurniture = CreateButtonWithFurniture(prefab);
                prefabPos += new Vector3(40, 0, 0);
                catalogueButtonWithFurniture.transform.SetParent(content.transform);
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
            content.gameObject.SetActive(false);  
        }
        yield return null;
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


    private void OnClickCategoryButton(string category)
    {
        foreach (Transform child in viewport.transform)
        {
            child.gameObject.SetActive(false);
        }

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
