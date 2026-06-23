// ---- Кастомный рендерер для меню (убираем яркий фон) ----
using System.Drawing;
using System.Windows.Forms;

class CustomMenuRenderer : ToolStripProfessionalRenderer
{
    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        if (e.Item.Selected)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(60, 60, 80)), e.Item.ContentRectangle);
        }
        else
        {
            base.OnRenderMenuItemBackground(e);
        }
    }
}