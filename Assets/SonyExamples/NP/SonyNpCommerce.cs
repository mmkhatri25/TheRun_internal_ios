using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_PS4
public class SonyNpCommerce : IScreen
{
    MenuLayout m_MenuCommerce;

    Sony.NP.Commerce.CategoryLabel firstSubCategory;

    List<Sony.NP.Commerce.ProductLabel> productLabels = new List<Sony.NP.Commerce.ProductLabel>();
    List<Sony.NP.Commerce.SkuLabel> purchasableSkuLabels = new List<Sony.NP.Commerce.SkuLabel>();

    Sony.NP.Commerce.ServiceEntitlementLabel firstConsumableEntitlement;

    public void AddProductId(Sony.NP.Commerce.ProductLabel id)
    {
        // Only add if not already in the list.
        for(int i = 0; i < productLabels.Count; i++)
        {
            if ( productLabels[i].Value == id.Value)
            {
                return;
            }
        }

        productLabels.Add(id);
    }

    public void AddPurchasableSkuId(Sony.NP.Commerce.SkuLabel id)
    {
        // Only add if not already in the list.
        for (int i = 0; i < purchasableSkuLabels.Count; i++)
        {
            if (purchasableSkuLabels[i].Value == id.Value)
            {
                return;
            }
        }

        purchasableSkuLabels.Add(id);
    }

    public SonyNpCommerce()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuCommerce;
    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }

    public void Process(MenuStack stack)
    {
        MenuUserProfiles(stack);
    }

    public void Initialize()
    {
        m_MenuCommerce = new MenuLayout(this, 450, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuCommerce.Update();

        if (m_MenuCommerce.AddItem("Get Categories", "Get the categories ."))
        {
            GetCategories();
        }

        if (m_MenuCommerce.AddItem("Get First SubCategory", "Get the first sub category returned from 'Get Categories' if one exists.", firstSubCategory.Value != null && firstSubCategory.Value.Length > 0))
        {
            GetFirstSubCategories();
        }

        if (m_MenuCommerce.AddItem("Get Products", "Get the root category products."))
        {
            GetProducts();
        }

        if (m_MenuCommerce.AddItem("Get Product Details", "Get the product details returned from 'Get Products' if any exist. This will include the additional product details.", productLabels.Count > 0))
        {
            GetProductDetails();
        }

        if (m_MenuCommerce.AddItem("Get Service Entitlements", "Get the list of service entitlements if any exist."))
        {
            GetServiceEntitlements();
        }

        if (m_MenuCommerce.AddItem("Consume First Service Entitlement", "Consume usage on a found service entitlement. Comsume usage for the first appropiate entitlement found.", firstConsumableEntitlement.Value != null && firstConsumableEntitlement.Value.Length > 0))
        {
            ConsumeServiceEntitlement();
        }

        if (m_MenuCommerce.AddItem("Display Category Browse Dialog", "This the category dialog. This will show the root category."))
        {
            DisplayCategoryBrowseDialog();
        }

        if (m_MenuCommerce.AddItem("Display Product Browse Dialog", "Display the product dialog for the first product returned from 'Get Products' if one exists.", productLabels.Count > 0))
        {
            DisplayProductBrowseDialog();
        }

        if (m_MenuCommerce.AddItem("Display Voucher Code Dialog", "Display the dialog for entering a voucher code."))
        {
            DisplayVoucherCodeInputDialog();
        }

        if (m_MenuCommerce.AddItem("Display Checkout Dialog", "Open the purchase dialog for the first purchasable SkuId if one exists. Use 'Get Product Details' first to find a suitable product.", purchasableSkuLabels.Count > 0))
        {
            DisplayCheckoutDialog();
        }

        if (m_MenuCommerce.AddItem("Display Download List Dialog", "Open the download list dialog. Use 'Get Product Details' first to find a suitable product.", purchasableSkuLabels.Count > 0))
        {
            DisplayDownloadListDialog();
        }

        if (m_MenuCommerce.AddItem("Display Join Plus Dialog", "Display the dialog to join PS Plus."))
        {
            DisplayJoinPlusDialog();
        }

        if (m_MenuCommerce.AddItem("Toggle PsStore Icon Display State", "Toggle the display, and the position, of the PS Store icon."))
        {
            SetPsStoreIconDisplayState();
        }

        if (m_MenuCommerce.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void GetCategories()
    {
        try
        {
            Sony.NP.Commerce.GetCategoriesRequest request = new Sony.NP.Commerce.GetCategoriesRequest();

            request.CategoryLabels = null;  // Get root category

            Sony.NP.Commerce.CategoriesResponse response = new Sony.NP.Commerce.CategoriesResponse();

            int requestId = Sony.NP.Commerce.GetCategories(request, response);
            OnScreenLog.Add("GetCategories Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetFirstSubCategories()
    {
        try
        {
            Sony.NP.Commerce.GetCategoriesRequest request = new Sony.NP.Commerce.GetCategoriesRequest();

            Sony.NP.Commerce.CategoryLabel[] categoryIds = new Sony.NP.Commerce.CategoryLabel[1];

            categoryIds[0] = firstSubCategory;

            request.CategoryLabels = categoryIds;

            Sony.NP.Commerce.CategoriesResponse response = new Sony.NP.Commerce.CategoriesResponse();

            int requestId = Sony.NP.Commerce.GetCategories(request, response);
            OnScreenLog.Add("GetCategories Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetProducts()
    {
        try
        {
            productLabels.Clear();

            Sony.NP.Commerce.GetProductsRequest request = new Sony.NP.Commerce.GetProductsRequest();

            request.ProductLabels = null;
            request.CategoryLabels = null;

            Sony.NP.Commerce.ProductsResponse response = new Sony.NP.Commerce.ProductsResponse();

            int requestId = Sony.NP.Commerce.GetProducts(request, response);
            OnScreenLog.Add("GetProducts Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetProductDetails()
    {
        try
        {
            Sony.NP.Commerce.GetProductsRequest request = new Sony.NP.Commerce.GetProductsRequest();

            request.ProductLabels = productLabels.ToArray();
            request.CategoryLabels = null;

            Sony.NP.Commerce.ProductsResponse response = new Sony.NP.Commerce.ProductsResponse();

            int requestId = Sony.NP.Commerce.GetProducts(request, response);
            OnScreenLog.Add("GetProducts Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetServiceEntitlements()
    {
        try
        {
            Sony.NP.Commerce.GetServiceEntitlementsRequest request = new Sony.NP.Commerce.GetServiceEntitlementsRequest();

            Sony.NP.Commerce.ServiceEntitlementsResponse response = new Sony.NP.Commerce.ServiceEntitlementsResponse();

            int requestId = Sony.NP.Commerce.GetServiceEntitlements(request, response);
            OnScreenLog.Add("GetServiceEntitlements Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void ConsumeServiceEntitlement()
    {
        try
        {
            Sony.NP.Commerce.ConsumeServiceEntitlementRequest request = new Sony.NP.Commerce.ConsumeServiceEntitlementRequest();

            OnScreenLog.Add("Comsume Entitlement : " + firstConsumableEntitlement.Value);

            request.ConsumedCount = 1;
            request.EntitlementLabel = firstConsumableEntitlement;
            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            int requestId = Sony.NP.Commerce.ConsumeServiceEntitlement(request, response);
            OnScreenLog.Add("ConsumeServiceEntitlement Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void DisplayCategoryBrowseDialog()
    {
        try
        {
            Sony.NP.Commerce.DisplayCategoryBrowseDialogRequest request = new Sony.NP.Commerce.DisplayCategoryBrowseDialogRequest();

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            int requestId = Sony.NP.Commerce.DisplayCategoryBrowseDialog(request, response);
            OnScreenLog.Add("DisplayCategoryBrowseDialog Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void DisplayProductBrowseDialog()
    {
        try
        {
            Sony.NP.Commerce.DisplayProductBrowseDialogRequest request = new Sony.NP.Commerce.DisplayProductBrowseDialogRequest();

            request.ProductLabel = productLabels[0];

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            int requestId = Sony.NP.Commerce.DisplayProductBrowseDialog(request, response);
            OnScreenLog.Add("DisplayProductBrowseDialog Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void DisplayVoucherCodeInputDialog()
    {
        try
        {
            Sony.NP.Commerce.DisplayVoucherCodeInputDialogRequest request = new Sony.NP.Commerce.DisplayVoucherCodeInputDialogRequest();

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            int requestId = Sony.NP.Commerce.DisplayVoucherCodeInputDialog(request, response);
            OnScreenLog.Add("DisplayVoucherCodeInputDialog Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void DisplayCheckoutDialog()
    {
        try
        {
            Sony.NP.Commerce.DisplayCheckoutDialogRequest request = new Sony.NP.Commerce.DisplayCheckoutDialogRequest();

            Sony.NP.Commerce.CheckoutTarget[] checkoutTargets = new Sony.NP.Commerce.CheckoutTarget[1];
            checkoutTargets[0].ProductLabel = productLabels[0];
            checkoutTargets[0].SkuLabel = purchasableSkuLabels[0];
            checkoutTargets[0].ServiceLabel = 0;

            OnScreenLog.Add("DisplayCheckoutDialog : First ProductLabel = " + checkoutTargets[0].ProductLabel.Value + " SkuLabel = " + checkoutTargets[0].SkuLabel.Value);

            request.Targets = checkoutTargets;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            int requestId = Sony.NP.Commerce.DisplayCheckoutDialog(request, response);
            OnScreenLog.Add("DisplayCheckoutDialog Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void DisplayDownloadListDialog()
    {
        try
        {
            Sony.NP.Commerce.DisplayDownloadListDialogRequest request = new Sony.NP.Commerce.DisplayDownloadListDialogRequest();

            Sony.NP.Commerce.DownloadListTarget[] downloadListTarget = new Sony.NP.Commerce.DownloadListTarget[purchasableSkuLabels.Count];

            for(int i = 0; i < purchasableSkuLabels.Count; i++)
            {
                downloadListTarget[i].ProductLabel = productLabels[i];
                downloadListTarget[i].SkuLabel = purchasableSkuLabels[i];
            }

            request.Targets = downloadListTarget;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            int requestId = Sony.NP.Commerce.DisplayDownloadListDialog(request, response);
            OnScreenLog.Add("DisplayDownloadListDialog Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void DisplayJoinPlusDialog()
    {
        try
        {
            Sony.NP.Commerce.DisplayJoinPlusDialogRequest request = new Sony.NP.Commerce.DisplayJoinPlusDialogRequest();

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            int requestId = Sony.NP.Commerce.DisplayJoinPlusDialog(request, response);
            OnScreenLog.Add("DisplayJoinPlusDialog Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    Sony.NP.Commerce.PsStoreIconPos iconPos = Sony.NP.Commerce.PsStoreIconPos.Center;
    bool showPsIcon = true;

    public void SetPsStoreIconDisplayState()
    {
        try
        {
            Sony.NP.Commerce.SetPsStoreIconDisplayStateRequest request = new Sony.NP.Commerce.SetPsStoreIconDisplayStateRequest();

            request.IconPosition = iconPos;
            request.ShowIcon = showPsIcon;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            int requestId = Sony.NP.Commerce.SetPsStoreIconDisplayState(request, response);
            OnScreenLog.Add("SetPsStoreIconDisplayState Async : Request Id = " + requestId);

            // Change the value for the next call.
            showPsIcon = !showPsIcon;
            if (showPsIcon == true)
            {
                // Change the position
                iconPos += 1;
                if ( iconPos > Sony.NP.Commerce.PsStoreIconPos.Right)
                {
                    iconPos = Sony.NP.Commerce.PsStoreIconPos.Center;
                }
            }
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {
        if (callbackEvent.Service == Sony.NP.ServiceTypes.Commerce)
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.NP.FunctionTypes.CommerceGetCategories:
                    OutputCategories(callbackEvent.Response as Sony.NP.Commerce.CategoriesResponse);
                    break;
                case Sony.NP.FunctionTypes.CommerceGetProducts:
                    OutputProducts(callbackEvent.Response as Sony.NP.Commerce.ProductsResponse);
                    break;
                case Sony.NP.FunctionTypes.CommerceGetServiceEntitlements:
                    OutputServiceEntitlements(callbackEvent.Response as Sony.NP.Commerce.ServiceEntitlementsResponse);
                    break;
                case Sony.NP.FunctionTypes.CommerceConsumeServiceEntitlement:
                    OutputConsumeServiceEntitlement(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;


                case Sony.NP.FunctionTypes.CommerceDisplayCategoryBrowseDialog:
                    OutputDisplayCategoryBrowseDialog(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.CommerceDisplayProductBrowseDialog:
                    OutputDisplayProductBrowseDialog(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.CommerceDisplayVoucherCodeInputDialog:
                    OutputDisplayVoucherCodeInputDialog(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.CommerceDisplayCheckoutDialog:
                    OutputDisplayCheckoutDialog(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.CommerceDisplayDownloadListDialog:
                    OutputeDisplayDownloadListDialog(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.CommerceDisplayJoinPlusDialog:
                    OutputDisplayJoinPlusDialog(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.CommerceSetPsStoreIconDisplayState:
                    OutputSetPsStoreIconDisplayState(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;

                default:
                    break;
            }
        }
    }

    private void OutputCategories(Sony.NP.Commerce.CategoriesResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Categories Response");

        if (response.Locked == false)
        {
            if (response.Categories == null)
            {
                OnScreenLog.Add("Num Catergories : 0");
            }
            else
            {
                OnScreenLog.Add("Num Catergories : " + response.Categories.Length);

                for (int i = 0; i < response.Categories.Length; i++)
                {
                    OnScreenLog.Add("    Catergory : " + i);
                    OutputCategory(response.Categories[i]);
                }

                if (response.Categories.Length > 0 &&
                    response.Categories[0].SubCategories != null &&
                    response.Categories[0].SubCategories.Length > 0)
                {
                    firstSubCategory = response.Categories[0].SubCategories[0].CategoryLabel;
                }
            }
        }
    }

    private void OutputCategory(Sony.NP.Commerce.Category category)
    {
        OnScreenLog.Add("        CategoryLabel : " + category.CategoryLabel.Value);
        OnScreenLog.Add("        CategoryName : " + category.CategoryName);
        OnScreenLog.Add("        CategoryDescription : " + category.CategoryDescription);
        OnScreenLog.Add("        ImageUrl : " + category.ImageUrl);
        OnScreenLog.Add("        CountOfProducts : " + category.CountOfProducts);

        if (category.SubCategories == null)
        {
            OnScreenLog.Add("        Num SubCatergories : 0");
        }
        else
        {
            OnScreenLog.Add("        Num SubCategories : " + category.SubCategories.Length);

            for (int i = 0; i < category.SubCategories.Length; i++)
            {
                OnScreenLog.Add("        SubCatergory : " + i);
                OutputSubCategory(category.SubCategories[i]);
            }
        }
    }

    private void OutputSubCategory(Sony.NP.Commerce.SubCategory subCategory)
    {
        OnScreenLog.Add("            CategoryLabel : " + subCategory.CategoryLabel.Value);
        OnScreenLog.Add("            CategoryName : " + subCategory.CategoryName);
        OnScreenLog.Add("            CategoryDescription : " + subCategory.CategoryDescription);
        OnScreenLog.Add("            ImageUrl : " + subCategory.ImageUrl);
    }

    private void OutputProducts(Sony.NP.Commerce.ProductsResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Products Response");

        if (response.Locked == false)
        {
            if (response.Products == null)
            {
                OnScreenLog.Add("Num Products : 0");
            }
            else
            {
                OnScreenLog.Add("Num Products : " + response.Products.Length);

                for (int i = 0; i < response.Products.Length; i++)
                {
                    OnScreenLog.Add("    Product : " + i);
                    OutputProduct(response.Products[i]);
                }
            }
        }
    }

    private void OutputProduct(Sony.NP.Commerce.Product product)
    {
        AddProductId(product.ProductLabel);

        OnScreenLog.Add("            ProductLabel : " + product.ProductLabel.Value);
        OnScreenLog.Add("            ProductName : " + product.ProductName);
        OnScreenLog.Add("            ImageUrl : " + product.ImageUrl);
        OnScreenLog.Add("            HasDetails : " + product.HasDetails);

        if (product.HasDetails == true)
        {
            OutputProductDetails(product.Details);
        }
    }

    private void OutputProductDetails(Sony.NP.Commerce.ProductDetails productDetails)
    {
        OnScreenLog.Add("            Product Details : ");
        OnScreenLog.Add("                ReleaseDate : " + productDetails.ReleaseDate);
        OnScreenLog.Add("                LongDescription : " + productDetails.LongDescription);
        OnScreenLog.Add("                SpName : " + productDetails.SpName);
        OnScreenLog.Add("                RatingSystemId : " + productDetails.RatingSystemId);
        OnScreenLog.Add("                RatingImageUrl : " + productDetails.RatingImageUrl);
   //     OnScreenLog.Add("                PurchasabilityStatus : " + productDetails.PurchasabilityStatus);
        OnScreenLog.Add("                StarRatingsTotal : " + productDetails.StarRatingsTotal);
        OnScreenLog.Add("                StarRatingScore : " + productDetails.StarRatingScore);

        if (productDetails.Skuinfo != null)
        {
            OnScreenLog.Add("                Skuinfo Length : " + productDetails.Skuinfo.Length);
            for (int i = 0; i < productDetails.Skuinfo.Length; i++)
            {
                OnScreenLog.Add("                SkuInfo : " + i);
                OutputSkuInfo(productDetails.Skuinfo[i]);
            }
        }
        else
        {
            OnScreenLog.Add("                Skuinfo : None");
        }

        if (productDetails.RatingDescriptors != null)
        {
            for (int i = 0; i < productDetails.RatingDescriptors.Length; i++)
            {
                if (productDetails.RatingDescriptors[i].Name.Length > 0)
                {
                    OnScreenLog.Add("                RatingDescriptors[] : " + i);
                    OnScreenLog.Add("                    Name : " + productDetails.RatingDescriptors[i].Name);
                    OnScreenLog.Add("                    ImageUrl : " + productDetails.RatingDescriptors[i].ImageUrl);
                }
            }
        }
    }

    private void OutputSkuInfo(Sony.NP.Commerce.SkuInfo skuInfo)
    {
        if (skuInfo.PurchasabilityStatus != Sony.NP.Commerce.PurchasabilityStatus.PurchasedCannotPurchaseAgain)
        {
            AddPurchasableSkuId(skuInfo.Label);
        }

        OnScreenLog.Add("                   Type : " + skuInfo.Type);
        OnScreenLog.Add("                   PurchasabilityStatus : " + skuInfo.PurchasabilityStatus);
        OnScreenLog.Add("                   Label : " + skuInfo.Label.Value);
        OnScreenLog.Add("                   Name : " + skuInfo.Name);
        OnScreenLog.Add("                   Price : " + skuInfo.Price);
        OnScreenLog.Add("                   IntPrice : " + skuInfo.IntPrice);
        OnScreenLog.Add("                   ConsumableUseCount : " + skuInfo.ConsumableUseCount);
    }

    private void OutputServiceEntitlements(Sony.NP.Commerce.ServiceEntitlementsResponse response)
    {
        if (response == null) return;

        firstConsumableEntitlement.Value = "";

        OnScreenLog.Add("Service Entitlements Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("TotalEntitlementsAvailable : " + response.TotalEntitlementsAvailable);

            if (response.Entitlements == null)
            {
                OnScreenLog.Add("Num Entitlements : 0");
            }
            else
            {
                OnScreenLog.Add("Num Entitlements : " + response.Entitlements.Length);

                for (int i = 0; i < response.Entitlements.Length; i++)
                {
                    OnScreenLog.Add("    Entitlement : " + i);
                    OutputEntitlement(response.Entitlements[i]);
                }
            }
        }
    }

    private void OutputEntitlement(Sony.NP.Commerce.ServiceEntitlement serviceEntitlement)
    {
        OnScreenLog.Add("            EntitlementLabel : " + serviceEntitlement.EntitlementLabel.Value);
        OnScreenLog.Add("            CreatedDate : " + serviceEntitlement.CreatedDate);
        OnScreenLog.Add("            ExpireDate : " + serviceEntitlement.ExpireDate);
        OnScreenLog.Add("            RemainingCount : " + serviceEntitlement.RemainingCount);
        OnScreenLog.Add("            ConsumedCount : " + serviceEntitlement.ConsumedCount);
        OnScreenLog.Add("            Type : " + serviceEntitlement.Type);

        if ( firstConsumableEntitlement.Value == null || firstConsumableEntitlement.Value.Length == 0 )
        {
            if (serviceEntitlement.RemainingCount > 0 )
            {
                firstConsumableEntitlement = serviceEntitlement.EntitlementLabel;
            }
        }
    }

    private void OutputConsumeServiceEntitlement(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Consume Service Entitlement Empty Response");

        OnScreenLog.Add("ReturnCode (Number remaining uses) : " + response.ReturnCodeValue);
    }

    private void OutputDisplayCategoryBrowseDialog(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Display Category Browse Dialog Empty Response");
    }

    private void OutputDisplayProductBrowseDialog(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Display Product Browse Dialog Empty Response");
    }

    private void OutputDisplayVoucherCodeInputDialog(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Display Voucher Code Input Dialog Empty Response");
    }

    private void OutputDisplayCheckoutDialog(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Display Checkout Dialog Empty Response");
    }

    private void OutputeDisplayDownloadListDialog(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Display Download List Dialog Empty Response");
    }

    private void OutputDisplayJoinPlusDialog(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Display Join Plus Dialog Empty Response");
    }

    private void OutputSetPsStoreIconDisplayState(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Set PsStore Icon Display State Empty Response");
    }

}
#endif
