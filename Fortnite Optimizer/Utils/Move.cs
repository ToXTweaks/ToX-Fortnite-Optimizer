using System.Drawing;
using System.Windows.Forms;

public class Move
{
    private Point _mouseDownLocation;
    private bool _isDragging;
    public Move(Form form, Control[] controls)
    {
        form.MouseDown += (sender, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _mouseDownLocation = e.Location;
            }
        };
        form.MouseMove += (sender, e) =>
        {
            if (_isDragging)
            {
                form.Cursor = Cursors.SizeAll;
                form.Opacity = 0.94;
                form.Location = new Point(
                    form.Location.X + e.X - _mouseDownLocation.X,
                    form.Location.Y + e.Y - _mouseDownLocation.Y
                );
            }
        };
        form.MouseUp += (sender, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                form.Cursor = Cursors.Default;
                form.Opacity = 0.98;
                _isDragging = false;
            }
        };
        foreach (var c in controls)
        {
            c.MouseDown += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    _isDragging = true;
                    _mouseDownLocation = e.Location;
                }
            };
            c.MouseMove += (sender, e) =>
            {
                if (_isDragging)
                {
                    form.Cursor = Cursors.SizeAll;
                    form.Opacity = 0.94;
                    form.Location = new Point(
                        form.Location.X + e.X - _mouseDownLocation.X,
                        form.Location.Y + e.Y - _mouseDownLocation.Y
                    );
                }
            };
            c.MouseUp += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    form.Cursor = Cursors.Default;
                    form.Opacity = 0.98;
                    _isDragging = false;
                }
            };
        }
    }
}