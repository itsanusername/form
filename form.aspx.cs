using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI.HtmlControls;
using System.Collections;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Data.SqlClient;


public partial class form : System.Web.UI.Page
{
    private string uploaddir;//direcory to save images
    private int maxtoupload;//maximum pics user can upload
    ArrayList imgpaths;//with ViewState is need to remember paths of uploaded imgs
    private string connectionString =
            System.Web.Configuration.WebConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString;
    //***************************************************
    private void Page_Init(object sender, EventArgs e)
    {
        if (!Request.IsAuthenticated)
        {
            Response.Redirect("LoggingIn.aspx");
        }
    }    
    //***************************************************
    private void Page_Load(object sender, EventArgs e)
    {        
        uploaddir = Server.MapPath("/Uploads");
        maxtoupload = 5;//maximum pictures to upload
        imgpaths = new ArrayList();
        //VIEWSTATE UPLOADS PATHS INTO ARRAY
        if (ViewState["imgpaths"] != null)
        {
            imgpaths = (ArrayList)ViewState["imgpaths"];
        }        
        //TEXT EDITOR, FORMS THE FONTS
        if (!IsPostBack)
        {
            //Add fonts' names into list
            InstalledFontCollection fonts = new InstalledFontCollection();
            foreach (System.Drawing.FontFamily family in fonts.Families)
            {
                ListItem item = new ListItem();
                item.Value = family.Name;
                lstfonts.Items.Add(item);
            }
            lstfonts.SelectedItem.Value = "Cambria";
            // Add data to the fontSizeList control.
            ListItemCollection fontSizes = new ListItemCollection();
            fontSizes.Add(new ListItem { Text = "мелкий", Value = "14" });
            fontSizes.Add(new ListItem { Text = "нормальный", Value = "18" });
            fontSizes.Add(new ListItem { Text = "средний", Value = "22" });
            fontSizes.Add(new ListItem {Text="крупный", Value="26" });
            lstsize.DataSource = fontSizes;
            lstsize.DataTextField = "Text";
            lstsize.DataValueField = "Value";
            lstsize.DataBind();
        }
        txbArticle.Font.Name = lstfonts.SelectedItem.Value;
        txbArticle.Font.Size = FontUnit.Parse(lstsize.SelectedItem.Value);
        if (!IsPostBack)
        {
            fillLists();
        }
        //Rexreate DATA OF TXTBOX CATEGORIES
        if (IsPostBack)
        {
            ReCreateTxtbxControls();
        }
    }
    //***************************************************
    private void Page_PreRender(object sender, EventArgs e)
    {
        string passstrtojscr = "";//STRING TO FORM ARRAY TO PASS INTO JSCR
        deleteImgs();
        //SAVING PATHS OF IMGS INTO VIEWSTATE
        if (imgpaths != null)
        {
            ViewState.Add("imgpaths", imgpaths);
            //passing all names of images to the JS array
            for (int i = 0; i < imgpaths.Count; i++)
            {
                passstrtojscr += "\"" + imgpaths[i] + "\"";
                if (i < imgpaths.Count - 1)
                {
                    passstrtojscr += ",";
                }
            }
            ClientScriptManager cs = Page.ClientScript;
            cs.RegisterArrayDeclaration("getarr", passstrtojscr);
        }
        else
        {
            ViewState["imgpaths"] = null;
        }
    }
    //****************************************************
    private void Page_Error(object sender, EventArgs e)
    {
        Exception exc = Server.GetLastError();
        if (exc is HttpException)
        {
            Server.ClearError();
            string prevPage = Request.UrlReferrer.ToString();
            Response.Redirect(prevPage);
        }
    }
    
    //***************************************************
    protected void uplbtn(object sender, EventArgs e)
    {
        if (fup.HasFiles)
        {
            lbl.Text = "";
            if (maxtoupload < fup.PostedFiles.Count)
            {
                lbl.Text = "можно загрузить не более " + maxtoupload.ToString();
            }
            for (int filecount = 0; filecount < fup.PostedFiles.Count; filecount++)
            {
                string fextension = "";
                //CHECKS FILES' EXTENSION
                fextension = System.IO.Path.GetExtension(fup.PostedFiles[filecount].FileName.ToLower());
                switch (fextension)
                {
                    case ".jpeg":
                    case ".jpg":
                    case ".png":
                    case ".bmp":
                    case ".gif":
                        {
                            if (imgpaths.Count < maxtoupload)
                            {
                                //SAVES IMGS AND RETURN SAVED NAME AS STRING INTO ARRAYLIST
                                imgpaths.Add(saveImages(filecount, fextension));
                            }
                            else
                            {
                                lbl.Text = "можно загрузить не более " + maxtoupload.ToString();
                            }
                        }
                        break;
                    default: lbl.Text = "Файл не подходит, попробуйте: .jpg .bmp .gif ";
                        return;
                }
            }
        }
        else
        {
            lbl.Text = "выберите файл";
        }
    }

    //GUID - NEW NAME FOR PICTURES, CONSISTS OF 10 FIRST CHARS
    private string guidname()
    {
        string str = "";
        Guid gu = Guid.NewGuid();
        str = gu.ToString("N").Substring(0, 10);
        return str;
    }
    //************↓↓↓↓↓↓↓↓↓ PICTURES ↓↓↓↓↓↓↓↓↓*************************
    
    //IT RESIZES IMAGES
    public static System.Drawing.Image ScaleImage(System.Drawing.Image image, int maxHeight)
    {
        var ratio = (double)maxHeight / image.Height;
        var newWidth = (int)(image.Width * ratio);
        var newHeight = (int)(image.Height * ratio);
        var newImage = new Bitmap(newWidth, newHeight);
        using (var g = Graphics.FromImage(newImage))
        {
            g.DrawImage(image, 0, 0, newWidth, newHeight);
          
        }
        return newImage;
    }

    //IT IS FOR COMPRESSING IMAGES, TO SET LOW QUALITY 
    private static ImageCodecInfo GetEncoderInfo(String mimeType)
    {
        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
        foreach (ImageCodecInfo codec in codecs)
        {
            if (codec.MimeType == mimeType)
            {
                return codec;
            }
        }        
        return null;
    }

    //SAVES PATHS OF PICTURES, AND RETURNS A STRING "SAVED_NAME.EXT" 
    private string saveImages(int filecount, string fextension)
    {
        string newimagename;
        string pathtocheck;
        newimagename = guidname();
        newimagename += fextension;
        pathtocheck = Path.Combine(uploaddir, newimagename);
        ////resizing before saving
        System.Drawing.Image objImage;
        Bitmap bmpPostedImage = new Bitmap(fup.PostedFiles[filecount].InputStream);
        ////// setting picture in low quality
        ImageCodecInfo jpgEncoder = GetEncoderInfo("image/jpeg");
        System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
        EncoderParameters myEncoderParameters = new EncoderParameters(1);
        EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 10L);
        myEncoderParameters.Param[0] = myEncoderParameter;
        ////// Resizing pictures
        if (fup.PostedFiles[filecount].ContentLength > 50000)
        {            
            objImage = ScaleImage(bmpPostedImage, 400);
        }
        else
        {
            objImage = new Bitmap(bmpPostedImage);
        }
        ////
        try
        {
            if (System.IO.File.Exists(pathtocheck))
            {
                while (System.IO.File.Exists(pathtocheck))
                {
                    newimagename = guidname() + fextension;
                    pathtocheck = Path.Combine(uploaddir, newimagename);
                }
                objImage.Save(pathtocheck, jpgEncoder, myEncoderParameters);
                //fup.PostedFiles[filecount].SaveAs(pathtocheck);
            }
            else
            {
                objImage.Save(pathtocheck, jpgEncoder, myEncoderParameters);
                //fup.PostedFiles[filecount].SaveAs(pathtocheck);
            }
        }
        catch (Exception err) { }
        return newimagename as string;
    }

    //THIS DELETES PICTURES
    private void deleteImgs()
    {
        if (Request.Form["hdn"] != null)
        {
            string delpics = Request.Form["hdn"].ToString();
            string imgname = "";
            foreach (char ch in delpics)
            {
                if (ch != ',')
                {
                    imgname += ch;
                }
                else
                {
                    try
                    {
                        string picpath = Path.Combine(uploaddir, imgname);
                        if (File.Exists(picpath))
                        {
                            File.Delete(picpath);
                            imgpaths.Remove(imgname);
                        }
                    }
                    catch (Exception err) { }
                    imgname = "";
                }
            }
            hdn.Value = "";
        }
    }
    //************↑↑↑↑↑↑↑↑ PICTURES ↑↑↑↑↑↑↑↑↑↑↑*************************

    //THIS SAVES DATA INTO DB
    protected void btnOk_Click(object sender, EventArgs e)
    {
        
        Response.Write(ddlcategory.SelectedValue + "<br/>");
        Response.Write(ddlcategory.SelectedIndex + "<br/>");
        //is there a picture, check it in the folder upload
        //because user could have deleted it
        //is there a text
        //symbols in text
        //then save
        int cmndexecuted;
        string insertSQL;
        string lastrow = "lastrow" + guidname();        
        SqlConnection connstr = new SqlConnection(connectionString);
        Label lbl2 = new Label();
        lbl2.ForeColor = System.Drawing.Color.Red;
        ph2.Controls.Add(lbl2);
        if (txbHeader.Text != "" 
            &&((hdn.Value == "" && imgpaths.Count != 0) || txbArticle.Text != "") 
            && ddlcategory.SelectedIndex!=0)
        {
            txbHeader.Text = txbHeader.Text.Replace("'", "&#39");
            txbHeader.Text = txbHeader.Text.Replace("<", "&#60");
            txbHeader.Text = txbHeader.Text.Replace(">", "&#62");
            //*****
            insertSQL = "declare @" + lastrow + " int = 1 ;\n";
            insertSQL += "if(select max(pk_Id) from post_text) is not null\n";
            insertSQL += "SET @" + lastrow + "=(select max(pk_Id)+1 from post_text)\n";
            insertSQL += "insert into post_text(pk_Id, p_date, p_header, p_text, category)\n";
            insertSQL += "values (@" + lastrow + ", DATEADD(HOUR, 10, GETUTCDATE()),N'" + txbHeader.Text + "',";
            if (txbArticle.Text != "")
            {
                txbArticle.Text = txbArticle.Text.Replace("'", "&#39");
                txbArticle.Text = txbArticle.Text.Replace("\"", "&#34");
                txbArticle.Text = txbArticle.Text.Replace("<", "&#60");
                txbArticle.Text = txbArticle.Text.Replace(">", "&#62");
                string str ="<p>"+txbArticle.Text;
                str = str.Replace(System.Environment.NewLine, "</p><p>");
                str += "</p>";
                //*****
                insertSQL += " N'" + str + "'," + ddlcategory.SelectedValue + ")\n";
            }
            else
            {
                insertSQL += "'',"+ddlcategory.SelectedValue+")\n";
            }
            if (imgpaths.Count != 0)
            {
                foreach (object pic in imgpaths)
                {
                    if (File.Exists(uploaddir +"/"+ pic.ToString()))
                    {
                        insertSQL += "insert into post_pic(fk_Id, pic_name)\n";
                        insertSQL += "values (@" + lastrow + ", '" + pic.ToString() + "')\n";
                    }
                }
            }
            foreach (TextBox tb in divcategories.Controls.OfType<TextBox>())
            {
                if (tb.Text !="")
                {
                    insertSQL += "insert into particular_category(cl_key, category)\n";
                    insertSQL += "values (@" + lastrow + ", N'" + tb.Text + "')\n";
                }
            }
            try
            {
                connstr.Open();
                SqlCommand cmnd = new SqlCommand(insertSQL, connstr);
                cmndexecuted = cmnd.ExecuteNonQuery();                
            }
            catch (Exception err)
            {
            }
            finally
            {                
                connstr.Close();
                if (connstr.State == System.Data.ConnectionState.Closed)
                {
                    Response.Redirect("~/Default.aspx");
                }
            }
        }
        else
        {
            lbl2.Text = "Нельзя опубликовать статью без заголовка. ";
            lbl2.Text += "Напишите статью или прикрепите фотографии. ";
            lbl2.Text += "Выберите категорию из основного списка. ";
        }
    }
    protected void btnCancel_Click(object sender, EventArgs e)
    {
        Response.Redirect("~/Default.aspx");
    }
    //THIS FILLS LIST OF CATEGORIES
    private void fillLists()
    { 
        //string selectSQL = "select * from main_categories order by category collate Cyrillic_General_CI_AS";//for cyrilic abc
        string selectSQL = "select * from main_categories order by category asc";
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand(selectSQL, conn);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            System.Data.DataSet dataset = new System.Data.DataSet();
            try
            {
                conn.Open();
                adapter.Fill(dataset, "category");
            }
            catch (Exception err) { }
            finally
            {
                conn.Close();
            }
            foreach (System.Data.DataRow row in dataset.Tables["category"].Rows)
            {
                ListItem newItem = new ListItem();
                newItem.Text = row["category"].ToString();
                newItem.Value = row["c_key"].ToString();
                ddlcategory.Items.Add(newItem);
            }
        }
        ddlcategory.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        ddlcategory.SelectedIndex = 0;
    }
    //ADDS PARTICULAR CATEGORY
    protected void btnpluscat_Click(object sender, EventArgs e)
    {
        if (ViewState["numControls"] == null)
        {
            ViewState["numControls"] = 1;
        }
        else 
        {
            int counter = (int)ViewState["numControls"];
            if (counter<5)
                counter++;
            ViewState["numControls"] = counter;
        }
        TextBox tbx = new TextBox();
        tbx.ID = "txbx" + ViewState["numControls"].ToString();
        tbx.CssClass = "tbxcategory";
        LiteralControl lit = new LiteralControl("<br/>");
        divcategories.Controls.AddAt(divcategories.Controls.IndexOf(btnpluscat), lit);
        divcategories.Controls.AddAt(divcategories.Controls.IndexOf(btnpluscat), tbx);
    }
    //if POSTBACK then RECREATE TXTBOXE CONTROLS
    protected void ReCreateTxtbxControls()
    {
        if (ViewState["numControls"] != null)
        {
            int counter = (int)ViewState["numControls"];
            string[] ctrls = Request.Form.ToString().Split('&');
            //amount of textbox controls less than 10
            for (int i = 1; i <= counter; i++)
            {
                LiteralControl lit = new LiteralControl("<br/>");
                TextBox tb = new TextBox();
                tb.ID = "txbx" + i.ToString();
                tb.CssClass = "tbxcategory";
                for (int j = 0; j < ctrls.Length; j++)
                {
                    if (ctrls[j].Contains("txbx" + i.ToString()))
                    {
                        string ctrlName = ctrls[j].Split('=')[0];
                        string ctrlValue = ctrls[j].Split('=')[1];
                        //Decode the Value
                        ctrlValue = Server.UrlDecode(ctrlValue);
                        tb.Text = ctrlValue;
                        break;
                    }
                }
                divcategories.Controls.AddAt(divcategories.Controls.IndexOf(btnpluscat), lit);
                divcategories.Controls.AddAt(divcategories.Controls.IndexOf(btnpluscat), tb);
            }
        }
    }
}