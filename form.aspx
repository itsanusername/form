<%@ Page Language="C#" AutoEventWireup="true" CodeFile="form.aspx.cs" Inherits="form" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>новая статья</title>
    <link href="/form.css?123456789" media="screen" rel="stylesheet" type="text/css" />
    <script type="text/javascript">
        window.onload = function getPics() {
            try {
                if (typeof getarr === 'undefined') {
                }
                else {
                    for (i = 0; i < getarr.length; i++) {
                        pic(i, getarr[i]);
                    }
                }
            }
            catch (err) {
            }
        }
        //to create uploaded imgs
        function pic(indx, picname) {           
            var ph1 = document.getElementById("<%=div2.ClientID%>");
            var im = document.createElement("img");
            var ptag = document.createElement("p");
            var br = document.createElement("br");
            var nod = document.createTextNode("удалить");
            im.setAttribute("src", "Uploads/" + picname);
            im.setAttribute("alt", picname);
            im.setAttribute("class", "added");
            ptag.id = "ptag" + indx.toString();
            ptag.setAttribute("class", "ptagadded");
            ptag.setAttribute("onclick", "imageDel(this)");
            ptag.appendChild(im);
            ptag.appendChild(br);
            ptag.appendChild(nod);
            ph1.appendChild(ptag);
        }
        //to delete uploaded imgs
        function imageDel(ptag) {
            var iden = ptag.id;
            var p = document.getElementById(iden);
            var picture = document.getElementById(iden).firstElementChild;
            console.log("Clicked id:" + picture.id + " src:" + picture.getAttribute("src") + " alt:" + picture.getAttribute("alt"));
            var delpicstr = document.getElementById("hdn");
            delpicstr.value += picture.getAttribute("alt") + ",";
            p.parentNode.removeChild(p);
        }
        //to change the font
        function changedFont(indx) {
            var txt = document.getElementById("txbArticle");
            txt.style.fontFamily = indx.value;
        }
        //to change the font size
        function changedSize(indx) {
            var txt = document.getElementById("txbArticle");
            txt.style.fontSize = indx.value ;
        }
        //forbidden chars
        function getChar(e) {
            var charcode;
            if(window.event) {                 
                charcode = e.keyCode;
            } else if(e.which){                  
                charcode = e.which;
            }
            if (charcode == 60 || charcode == 62) {
                alert("ИСПОЛЬЗУЙТЕ ДРУГИЕ СИМВОЛЫ ВМЕСТО: " + String.fromCharCode(charcode));
                return false;
            } else {
                return true;
            }            
        }
    </script>
</head>
<body>
    <h1>    Новая статья    </h1>
    <form id="frm1" runat="server">
        <!--Header-->
        <div id="div1">
            <h4>Заголовок:</h4> &nbsp; 
            <asp:TextBox ID="txbHeader" runat="server" TextMode="SingleLine" 
                CssClass="rndcrnr" Width="100%" 
                Font-Bold="True" Font-Size="Large" 
                Font-Names="Segoe UI"
                onkeypress="return getChar(event)"></asp:TextBox>
        </div>
        <!--Categories-->
        <div id="divcategories" runat="server">
            <h4>Укажите категорию(и) основные, и не относящиеся к ним.</h4><br/>
            Основные:<br/>
            <asp:DropDownList ID="ddlcategory" runat="server" CssClass="tbxcategory"></asp:DropDownList>&nbsp;
            <br/>Частные (свое описание):<br/>
            <asp:TextBox ID="TextBox1" runat="server" CssClass="tbxcategory"></asp:TextBox>&nbsp;
            <asp:Button ID="btnpluscat" runat="server" Text="добавить" OnClick="btnpluscat_Click" />
        </div>     
        <!--FOTO-->
        <div id="div2" runat="server" visible="true">
            <h4>Прикрепить фото:</h4> &nbsp; 
            <asp:FileUpload ID="fup" runat="server" AllowMultiple="True" BackColor="#CCCCCC" accept="image/*"/>
            &nbsp;&nbsp;&nbsp;
            <asp:Button ID="btnUpload" runat="server" Text="Прикрепить" Height="25px" OnClick="uplbtn"/><br />
            <asp:Label ID="lbl" runat="server" ForeColor="#ff0000"></asp:Label>
            <br />
            <asp:PlaceHolder ID="ph1" runat="server" ClientIDMode="Static"></asp:PlaceHolder>
            <input id="hdn" type="hidden" runat="server" value="" />
        </div>
        <!--Article-->
        <div id="div3">
            <h4>Статья</h4><br/>
            Шрифт:&nbsp;            
            <asp:DropDownList ID="lstfonts" runat="server" 
               onchange="changedFont(this)"></asp:DropDownList>&nbsp;   
            Размер шрифта:&nbsp;   
            <asp:DropDownList ID="lstsize" runat="server"
                onchange="changedSize(this)"></asp:DropDownList><br /><br />
            <asp:TextBox ID="txbArticle" runat="server" TextMode="MultiLine"
                CssClass="rndcnr" Width="100%" Height="316px"
                placeholder="ВАША СТАТЬЯ ИЛИ КОММЕНТАРИИ К ФОТО" 
                onkeypress="return getChar(event)"></asp:TextBox><br />
            <asp:Button ID="btnOk" runat="server" Text="OK" OnClick="btnOk_Click" />&nbsp;&nbsp;&nbsp;
            <asp:Button ID="btnCancel" runat="server" Text="Отмена" OnClick="btnCancel_Click" /><br />
            <asp:PlaceHolder ID="ph2" runat="server">
            </asp:PlaceHolder>
        </div>
         <!--Setting Focus-->
        <script type="text/javascript">
                if (document.getElementById("txbHeader").value === "") {
                    document.getElementById("txbHeader").focus();
                }
                else {
                    document.getElementById("txbArticle").focus();
                }
        </script>
        
    </form>

</body>   
</html>
