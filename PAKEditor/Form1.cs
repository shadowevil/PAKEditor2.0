using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using static PAKEditor.Form1;
using Timer = System.Windows.Forms.Timer;

namespace PAKEditor
{
    public class Form1 : Form
    {
        private ToolStrip toolBox;
        private ToolStripDropDownButton file;
        private Panel PictureView;
        private Panel AnimationView;
        private Panel RectangleDataView;
        private TextBox RectangleDataBox;
        private Image? SelectedSpriteImage;
        private Sprite? SelectedSprite;
        private SpriteRect? SelectedRect;
        private int SelectedRectIndex = -1;
        private int SelectedParentNode = -1;
        private bool IsRemovingBackground = false;
        private List<Color> SelectedPixels = new List<Color>();
        private Rectangle SelectedPixelRect = Rectangle.Empty;

        private Pen yellowPen = new Pen(Color.Yellow, 1) { Alignment = System.Drawing.Drawing2D.PenAlignment.Outset };
        private Pen redPen = new Pen(Color.Red, 1) { Alignment = System.Drawing.Drawing2D.PenAlignment.Outset };

        //private DateTime totalAnimationTime;
        //private Timer animationRefreshRate;

        private TreeView SpriteList;

        List<Sprite> sprites = new List<Sprite>();
        private string? OpenFileLocation { get; set; } = null;

        public Form1()
        {
            InitWindowParameters();
            InitFormComponents();
        }

        private void WinMain_SizeChanged(object? sender, EventArgs e)
        {
            PictureView.Width = this.Width - 266;
            PictureView.Refresh();
        }

        private void InitWindowParameters()
        {
            this.MinimumSize = new Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.SizeChanged += WinMain_SizeChanged;
            this.Text = "PAK Editor";
        }

        private void InitFormComponents()
        {
            //totalAnimationTime = DateTime.Now;
            //animationRefreshRate = new Timer();
            //animationRefreshRate.Interval = 16;
            //animationRefreshRate.Tick += AnimationRefreshRate_Tick;
            //SpriteArray = new List<Sprite>();

            Panel PictureViewContainer = new Panel();
            {
                PictureViewContainer.BackColor = Color.FromArgb(240, 240, 240);
                PictureViewContainer.Dock = DockStyle.Fill;
                PictureViewContainer.BorderStyle = BorderStyle.FixedSingle;
                PictureViewContainer.Name = "PictureViewContainer";
                PictureViewContainer.AutoScroll = true;
                PictureViewContainer.Resize += (sender, e) =>
                {
                    if (PictureView == null) return;
                    PictureView.MinimumSize = new Size((sender as Panel)!.Width, (sender as Panel)!.Height);
                };
                typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty
                            | BindingFlags.Instance | BindingFlags.NonPublic, null,
                            PictureViewContainer, new object[] { true });
            }
            this.Controls.Add(PictureViewContainer);

            PictureView = new Panel();
            {
                PictureView.BackColor = Color.FromArgb(240, 240, 240);
                PictureView.BorderStyle = BorderStyle.FixedSingle;
                //PictureView.Width = this.Width - 266;
                PictureView.Name = "PictureView";
                PictureView.Paint += PictureView_Paint;
                typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty
                            | BindingFlags.Instance | BindingFlags.NonPublic, null,
                            PictureView, new object[] { true });
            }
            PictureViewContainer.Controls.Add(PictureView);

            RectangleDataView = new Panel();
            {
                RectangleDataView.BackColor = Color.FromArgb(225, 225, 225);
                RectangleDataView.Dock = DockStyle.Bottom;
                RectangleDataView.Height = 150;
                RectangleDataView.Name = "RectangleDataView";
                RectangleDataView.Paint += RectangleDataView_Paint;
            }
            this.Controls.Add(RectangleDataView);

            RectangleDataBox = new TextBox();
            {
                RectangleDataBox.Width = 100;
                RectangleDataBox.Location = new Point(75, 25);
                RectangleDataBox.Enabled = false;
                RectangleDataBox.Name = "txtBox_X";
            }
            RectangleDataView.Controls.Add(RectangleDataBox);

            RectangleDataBox = new TextBox();
            {
                RectangleDataBox.Width = 100;
                RectangleDataBox.Location = new Point(75, 75);
                RectangleDataBox.Enabled = false;
                RectangleDataBox.Name = "txtBox_Y";
            }
            RectangleDataView.Controls.Add(RectangleDataBox);

            RectangleDataBox = new TextBox();
            {
                RectangleDataBox.Width = 100;
                RectangleDataBox.Location = new Point(300, 25);
                RectangleDataBox.Enabled = false;
                RectangleDataBox.Name = "txtBox_W";
            }
            RectangleDataView.Controls.Add(RectangleDataBox);

            RectangleDataBox = new TextBox();
            {
                RectangleDataBox.Width = 100;
                RectangleDataBox.Location = new Point(300, 75);
                RectangleDataBox.Enabled = false;
                RectangleDataBox.Name = "txtBox_H";
            }
            RectangleDataView.Controls.Add(RectangleDataBox);

            RectangleDataBox = new TextBox();
            {
                RectangleDataBox.Width = 100;
                RectangleDataBox.Location = new Point(500, 25);
                RectangleDataBox.Enabled = false;
                RectangleDataBox.Name = "txtBox_PivX";
            }
            RectangleDataView.Controls.Add(RectangleDataBox);

            RectangleDataBox = new TextBox();
            {
                RectangleDataBox.Width = 100;
                RectangleDataBox.Location = new Point(500, 75);
                RectangleDataBox.Enabled = false;
                RectangleDataBox.Name = "txtBox_PivY";
            }
            RectangleDataView.Controls.Add(RectangleDataBox);

            AnimationView = new Panel();
            {
                AnimationView.BackColor = Color.FromArgb(200, 200, 200);
                AnimationView.BorderStyle = BorderStyle.FixedSingle;
                AnimationView.Width = 200;
                AnimationView.Height = 200;
                AnimationView.Visible = false;
                typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty
                            | BindingFlags.Instance | BindingFlags.NonPublic, null,
                            AnimationView, new object[] { true });
            }
            PictureView.Controls.Add(AnimationView);


            SpriteList = new TreeView();
            {
                SpriteList.BackColor = Color.FromArgb(225, 225, 225);
                SpriteList.Dock = DockStyle.Left;
                SpriteList.Width = 250;
                SpriteList.AfterSelect += SpriteList_AfterSelect;
                SpriteList.NodeMouseClick += SpriteList_NodeMouseClick;
                SpriteList.Scrollable = true;
                SpriteList.HideSelection = false;
                SpriteList.DrawMode = TreeViewDrawMode.OwnerDrawText;
                SpriteList.DrawNode += (object? sender, DrawTreeNodeEventArgs e) =>
                {
                    e.DrawDefault = true;
                    if (e.Node!.IsSelected)
                    {
                        e.Node.ForeColor = Color.White;
                    }
                    else
                    {
                        e.Node.ForeColor = Color.Black;
                    }
                };
            }
            this.Controls.Add(SpriteList);

            toolBox = new ToolStrip();
            {
                toolBox.GripStyle = ToolStripGripStyle.Hidden;
                toolBox.BackColor = Color.FromArgb(230, 230, 230);
                toolBox.Dock = DockStyle.Top;

                file = new ToolStripDropDownButton("File");
                file.DropDownItems.Add("New", null, FileNew_Click);
                file.DropDownItems.Add("Open", null, FileOpen_Click);
                file.DropDownItems.Add(new ToolStripSeparator());
                file.DropDownItems.Add("Import All Sprites", null, Import_All_Sprites);
                file.DropDownItems.Add("Extract All Sprites", null, Extract_All_Sprites);
                file.DropDownItems.Add("Remove All Backgrounds", null, FileRemoveAllBackgrounds_Click);
                file.DropDownItems.Add("Remove All Backgrounds By Color", null, FileRemoveAllBackgroundsByColor_Click);
                file.DropDownItems.Add(new ToolStripSeparator());
                file.DropDownItems.Add("Add Sprite", null, FileAddSprite_Click);
                file.DropDownItems.Add("Add Pak File", null, FileAddPakFile_Click);
                file.DropDownItems.Add(new ToolStripSeparator());
                file.DropDownItems.Add("Save As", null, Save_Click);
                file.DropDownItems.Add(new ToolStripSeparator());
                file.DropDownItems.Add("Exit", null, null);

                toolBox.Items.Add(file);
            }
            this.Controls.Add(toolBox);

            ToggleMenuItem(3, false);
            ToggleMenuItem(4, false);
            ToggleMenuItem(5, false);
            ToggleMenuItem(6, false);
            ToggleMenuItem(8, false);
            ToggleMenuItem(9, false);
            ToggleMenuItem(11, false);

            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
        }

        private void FileAddPakFile_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = ".PAK Data Files (*.pak)|*.pak";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    List<Sprite> newSprites = new List<Sprite>();
                    BitReader reader = new BitReader(ofd.FileName);

                    if (reader.ReadString(17, 8) != "<Pak file header>")
                        throw new InvalidDataException("Invalid PAK file, no header found!");
                    reader.SeekBytes(reader.BytePosition + 3);
                    int SpriteCount = reader.ReadInt32();
                    for (int i = sprites.Count(); i < SpriteCount + sprites.Count(); i++)
                    {
                        newSprites.Add(new Sprite()
                        {
                            ID = i
                        });
                        SpriteList.Nodes.Add("Sprite " + i);
                    }

                    for (int i = 0; i < newSprites.Count; i++)
                    {
                        newSprites[i].StartBytePosition = reader.ReadUInt32();
                        newSprites[i].EndBytePosition = reader.ReadUInt32();
                    }

                    if (reader.ReadString(20, 8) != "<Sprite File Header>")
                        throw new InvalidDataException("Invalid or corrupt PAK file, no Sprite File Header found!");

                    foreach (var sprite in newSprites)
                    {
                        var nodeList = SpriteList.Nodes[sprite.ID];
                        reader.SeekBytes(sprite.StartBytePosition + 100);
                        sprite.FrameCount = reader.ReadInt32();
                        sprite.Rects = new List<SpriteRect>();
                        for (int i = 0; i < sprite.FrameCount; i++)
                        {
                            nodeList.Nodes.Add(nodeList.GetNodeCount(false).ToString());
                            sprite.Rects.Add(new SpriteRect()
                            {
                                x = reader.ReadInt16(),
                                y = reader.ReadInt16(),
                                width = reader.ReadInt16(),
                                height = reader.ReadInt16(),
                                pivx = reader.ReadInt16(),
                                pivy = reader.ReadInt16()
                            });
                        }
                        sprite.BitmapStartBytePosition = (uint)(sprite.StartBytePosition + (108 + (12 * sprite.FrameCount)));
                        reader.SeekBytes(sprite.BitmapStartBytePosition);
                        sprite.PNGBytes = reader.ReadBytes(sprite.EndBytePosition - (uint)(108 + (12 * sprite.FrameCount))).ToArray();

                        using (Image img = Image.FromStream(new MemoryStream(sprite.PNGBytes)))
                            sprite.bitmap = new Bitmap(img);
                    }

                    sprites.AddRange(newSprites);
                }
            }
        }

        private void Import_All_Sprites(object? sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string folderPath = fbd.SelectedPath;
                string newFolderPath = Path.Combine(folderPath + "\\");
                if (!Directory.Exists(newFolderPath))
                {
                    MessageBox.Show("Invalid directory, does not exist!. Unable to Import!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                List<string> SpriteFiles = Directory.GetFiles(newFolderPath)
                    .Where(x => Path.GetFileName(x).ToLower().StartsWith("sprite ") && Path.GetExtension(x) != ".SpriteRect")
                    .OrderBy(n => int.Parse(Path.GetFileNameWithoutExtension(n).Substring("Sprite ".Length)))
                    .ToList();

                if (SpriteFiles.Count <= 0)
                {
                    MessageBox.Show("Invalid directory, no files exist. Unable to import!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if(SpriteFiles.Count() < sprites.Count())
                {
                    MessageBox.Show("Invalid directory, file count is off or no files exist with the proper naming scheme. Unable to import!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool ShouldImportSpriteRects = false;
                bool ShouldAddNewSpritesAndRects = false;
                List<string> SpriteRectFiles = Directory.GetFiles(newFolderPath)
                    .Where(x => Path.GetFileName(x).ToLower().StartsWith("sprite ") && Path.GetExtension(x) == ".SpriteRect")
                    .OrderBy(n => int.Parse(Path.GetFileNameWithoutExtension(n).Substring("Sprite ".Length)))
                    .ToList();

                List<string> addSprites = new List<string>();
                List<string> addSpriteRects = new List<string>();

                if(SpriteFiles.Count() < SpriteRectFiles.Count() && SpriteRectFiles.Count() > 0)
                {
                    MessageBox.Show("Invalid Sprite to SpriteRect count... Unable to Import!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                } else if(SpriteFiles.Count() == SpriteRectFiles.Count())
                {
                    ShouldImportSpriteRects = true;
                    SpriteList.Nodes.Clear();

                    if (SpriteRectFiles.Count() - sprites.Count() > 0)
                    {
                        ShouldAddNewSpritesAndRects = true;
                        addSpriteRects.AddRange(SpriteRectFiles.GetRange(SpriteRectFiles.Count() - sprites.Count(), SpriteRectFiles.Count() - sprites.Count()));
                        addSprites.AddRange(SpriteFiles.GetRange(SpriteFiles.Count() - sprites.Count(), SpriteFiles.Count() - sprites.Count()));
                    }
                }

                for (int i=0;i<sprites.Count(); i++) {
                    Sprite spr = sprites[i];
                    if (!File.Exists(newFolderPath + $"Sprite {spr.ID}.png"))
                    {
                        MessageBox.Show($"An error has occur'd file does not exists, \'Sprite {spr.ID}.png\'", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if(ShouldImportSpriteRects)
                    {
                        if (!File.Exists(newFolderPath + $"Sprite {spr.ID}.SpriteRect"))
                        {
                            MessageBox.Show($"An error has occur'd file does not exists, \'Sprite {spr.ID}.SpriteRect\'", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    spr.bitmap = new Bitmap(Image.FromFile(SpriteFiles[i]));
                    using (MemoryStream ms = new MemoryStream())
                    {
                        switch (Path.GetExtension(SpriteFiles[i]).ToLower())
                        {
                            case ".png":
                                spr.bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                spr.PNGBytes = ms.ToArray();
                                break;
                            case ".bmp":
                                spr.bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                spr.PNGBytes = ms.ToArray();
                                break;
                        }
                        ms.Close();
                    }

                    if (ShouldImportSpriteRects)
                    {
                        SpriteRect[]? rects = JsonConvert.DeserializeObject<SpriteRect[]>(File.ReadAllText(SpriteRectFiles[i]));
                        if(rects == null)
                        {
                            MessageBox.Show($"An error has occur'd unable to import \'Sprite {spr.ID}.SpriteRect\'", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        spr.Rects = rects.ToList();

                        SpriteList.Nodes.Add("Sprite " + i);
                        foreach(SpriteRect rect in rects)
                        {
                            SpriteList.Nodes[i].Nodes.Add(SpriteList.Nodes[i].GetNodeCount(false).ToString());
                        }
                    }
                }

                if (ShouldAddNewSpritesAndRects)
                {
                    for (int i = 0; i < addSprites.Count(); i++)
                    {
                        SpriteRect[]? rects = JsonConvert.DeserializeObject<SpriteRect[]>(File.ReadAllText(addSpriteRects[i]));
                        if (rects == null)
                        {
                            MessageBox.Show($"An error has occur'd unable to import \'Sprite {i}.SpriteRect\'", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        Sprite spr = new Sprite()
                        {
                            ID = sprites.Count() > 0 ? sprites.Select(x => x.ID).Max() + 1 : 0,
                            Rects = rects.ToList(),
                            FrameCount = rects.Count(),
                            PNGBytes = Path.GetExtension(addSprites[i]).ToLower() == ".png" ? File.ReadAllBytes(addSprites[i]) : []
                        };
                        spr.bitmap = new Bitmap(Image.FromStream(new MemoryStream(spr.PNGBytes)));

                        sprites.Add(spr);

                        var node = SpriteList.Nodes.Add("Sprite " + spr.ID);
                        foreach (SpriteRect rect in rects)
                        {
                            node.Nodes.Add(node.GetNodeCount(false).ToString());
                        }
                    }
                }
                PictureView.Refresh();
            }
        }

        private void Extract_All_Sprites(object? sender, EventArgs e)
        {
            using(FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if(fbd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string folderPath = fbd.SelectedPath;
                string newFolderPath = Path.Combine(folderPath + "\\" + Path.GetFileNameWithoutExtension(OpenFileLocation)) + "\\";
                if (!Directory.Exists(newFolderPath))
                    Directory.CreateDirectory(newFolderPath);

                List<string> files = Directory.GetFiles(newFolderPath).ToList();
                if(files.Count > 0)
                {
                    MessageBox.Show("Invalid directory, files already exist. Unable to extract!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool ShouldExportRects = false;
                if (MessageBox.Show("Should export Sprite Rectangles?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    ShouldExportRects = true;
                }

                int OffsetAmount = 0;
                if(MessageBox.Show("Should offset file iteration?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (InputDialog.ShowInputDialog("Select Offset", "Input an offset for the files to be saved with") is string offsetStr)
                    {
                        if (!int.TryParse(offsetStr, out OffsetAmount))
                            OffsetAmount = 0;
                    }
                }

                foreach(Sprite spr in sprites)
                {
                    if(File.Exists(newFolderPath + $"Sprite {spr.ID}.png"))
                    {
                        MessageBox.Show($"An error has occur'd file already exists, \'Sprite {spr.ID}.png\'", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    using(MemoryStream ms = new MemoryStream(spr.PNGBytes))
                    using(Bitmap bmp = new Bitmap(Image.FromStream(ms)))
                    {
                        bmp.Save($"{newFolderPath}Sprite {spr.ID + OffsetAmount}.png", ImageFormat.Png);
                    }

                    if (ShouldExportRects)
                    {
                        string rect = JsonConvert.SerializeObject(spr.Rects);
                        File.WriteAllText($"{newFolderPath}Sprite {spr.ID + OffsetAmount}.SpriteRect", rect);
                    }
                }
            }
        }

        private MouseEventHandler? pictureView_RemoveAllByColor_Click = null;

        private void FileRemoveAllBackgroundsByColor_Click(object? sender, EventArgs e)
        {
            if (SelectedSprite == null)
                return;
            if (SelectedSpriteImage == null)
                return;

            removeBackground_Click(SelectedParentNode, true);

            pictureView_RemoveAllByColor_Click = (sender, e) => PictureView_RemoveAllByColor_Click(sender, e, SelectedParentNode);

            PictureView.MouseClick += pictureView_RemoveAllByColor_Click;
        }

        private void PictureView_RemoveAllByColor_Click(object? sender, MouseEventArgs e, int selectedSpriteIndex)
        {
            // Get the pixel color under the mouse and do something with it
            Color c = GetPixelColorUnderMouse(e);
            if (c != Color.Empty)
            {
                Point pixelCoord = GetPixelCoordinates();
                DialogResult dr = MessageBox.Show($"Color under mouse at ({pixelCoord.X}, {pixelCoord.Y}): {c}", "Should remove?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    for (int i = 0; i < sprites.Count(); i++)
                    {
                        DoRemoveBackground(c, i);
                    }
                }
            }
            else
            {
                MessageBox.Show("Selected Pixel outside the bounds of the image");
            }

            // Unsubscribe from the event after the click
            PictureView.MouseClick -= pictureView_RemoveAllByColor_Click;
            PictureView.MouseMove -= pictureView_MouseMoveHandler;
            Cursor = Cursors.Default;
            IsRemovingBackground = false;
            PictureView.Refresh();
        }

        private void FileRemoveAllBackgrounds_Click(object? sender, EventArgs e)
        {
            List<Color> FirstPixelColors = new List<Color>();

            for (int i = 0; i < sprites.Count; i++)
            {
                if (sprites[i].bitmap == null)
                    throw new NullReferenceException($"Invalid bitmap, this shouldn't happen as we should be loading the image before using!");

                FirstPixelColors.Add(sprites[i].bitmap!.GetPixel(0, 0));
            }

            if(FirstPixelColors.Count() != sprites.Count())
            {
                MessageBox.Show("Invalid number of pixel colors vs sprites, canceling...", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

#if DEBUG
            string colorsText = string.Empty;
            int indexFound = 0;
            foreach (Color c in FirstPixelColors)
            {
                colorsText += $"SpriteIndex: {indexFound++}[R: {c.R}, G: {c.G}, B: {c.B}, A: {c.A}]\n";
            }

            MessageBox.Show(colorsText);
#endif

            if (FirstPixelColors.Distinct().Count() != 1)
            {
                DialogResult dr = MessageBox.Show($"Warning!\n\nNot all pixel colors are the same, do you wish to continue?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.No)
                {
                    return;
                }
            }

            for (int i = 0; i < FirstPixelColors.Count(); i++)
            {
                DoRemoveBackground(FirstPixelColors[i], i);
            }
            PictureView.Refresh();
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (SelectedParentNode == -1)
                return;

            if(e.Control && e.KeyCode == Keys.G)
            {
                if (e.KeyCode == Keys.G)
                {
                    IsRemovingBackground = true;
                    removeBackground_Click(SelectedParentNode);
                }
            }

            if (e.Control && e.KeyCode == Keys.Delete)
            {
                DeleteByRange();
            } else if (e.KeyCode == Keys.Delete && SelectedRect == null)
            {
                deleteSprite_Click(SelectedParentNode);
            }
        }

        private void DeleteByRange()
        {
            if(RangeInputDialog.ShowInputDialog("Delete by range", "Select a valid range of sprites") is Range range)
            {
                sprites.RemoveRange(range.Start.Value, range.End.Value - range.Start.Value + 1);

                SpriteList.Nodes.Clear();

                for(int i=0;i < sprites.Count;i++)
                {
                    sprites[i].ID = i;

                    var node = SpriteList.Nodes.Add($"Sprite {i}");

                    for (int x = 0; x < sprites[i].Rects.Count; x++)
                    {
                        node.Nodes.Add(x.ToString());
                    }
                }
            }
        }

        private void FileAddSprite_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = string.Join('|', [
                    ".png Portable Network Graphics File (*.png)|*.png",
                        ".bmp Bitmap File (*.bmp)|*.bmp"
                    ]);

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Sprite spr = new Sprite()
                    {
                        ID = sprites.Count() > 0 ? sprites.Select(x => x.ID).Max() + 1 : 0,
                        Rects = new List<SpriteRect>(),
                        FrameCount = 0,
                        PNGBytes = Path.GetExtension(ofd.FileName).ToLower() == ".png" ? File.ReadAllBytes(ofd.FileName) : []
                    };
                    spr.bitmap = new Bitmap(Image.FromStream(new MemoryStream(spr.PNGBytes)));
                    sprites.Add(spr);
                    SpriteList.Nodes.Add("Sprite " + spr.ID);
                    SpriteList.SelectedNode = SpriteList.Nodes[spr.ID];
                    PictureView.Refresh();
                }
            }
        }

        private void FileNew_Click(object? sender, EventArgs e)
        {
            ClearTable();

            ToggleMenuItem(3, true);
            ToggleMenuItem(4, true);
            ToggleMenuItem(5, true);
            ToggleMenuItem(6, true);
            ToggleMenuItem(8, true);
            ToggleMenuItem(9, true);
            ToggleMenuItem(11, true);

            FileAddSprite_Click(sender, e);
        }

        private void Save_Click(object? sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = ".PAK Data Files (*.pak)|*.pak";
                if (!string.IsNullOrEmpty(OpenFileLocation))
                {
                    sfd.FileName = Path.GetFileName(OpenFileLocation);
                }

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(sfd.FileName, SavePAKFile());
                    MessageBox.Show($"{sfd.FileName} saved!");
                }
            }
        }

        private byte[] SavePAKFile()
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(Encoding.ASCII.GetBytes("<Pak file header>").ToArray());
            bytes.AddRange(new byte[3]);
            bytes.AddRange(System.BitConverter.GetBytes(sprites.Count()));

            int SpriteFileHeaderLocation = 24 + sprites.Count() * 8;
            int prevPNGBytesLength = 0;
            for (int i = 0; i < sprites.Count(); i++)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Bitmap clone = new Bitmap(sprites[i].bitmap!.Width, sprites[i].bitmap!.Height, PixelFormat.Format32bppArgb);

                    // Lock the original bitmap's bits
                    BitmapData originalData = sprites[i].bitmap!.LockBits(
                        new Rectangle(0, 0, sprites[i].bitmap!.Width, sprites[i].bitmap!.Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format32bppArgb);

                    // Lock the clone bitmap's bits
                    BitmapData cloneData = clone.LockBits(
                        new Rectangle(0, 0, clone.Width, clone.Height),
                        ImageLockMode.WriteOnly,
                        PixelFormat.Format32bppArgb);

                    int bytesPerPixel = Image.GetPixelFormatSize(originalData.PixelFormat) / 8;
                    int height = originalData.Height;
                    int width = originalData.Width;

                    unsafe
                    {
                        byte* originalScan0 = (byte*)originalData.Scan0.ToPointer();
                        byte* cloneScan0 = (byte*)cloneData.Scan0.ToPointer();

                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                byte* originalPixel = originalScan0 + y * originalData.Stride + x * bytesPerPixel;
                                byte* clonePixel = cloneScan0 + y * cloneData.Stride + x * bytesPerPixel;

                                byte alpha = originalPixel[3];

                                // Premultiply the RGB values by the alpha
                                clonePixel[0] = (byte)((originalPixel[0] * alpha) / 255); // B
                                clonePixel[1] = (byte)((originalPixel[1] * alpha) / 255); // G
                                clonePixel[2] = (byte)((originalPixel[2] * alpha) / 255); // R
                                clonePixel[3] = alpha; // A
                            }
                        }
                    }

                    // Unlock the bits
                    sprites[i].bitmap!.UnlockBits(originalData);
                    clone.UnlockBits(cloneData);

                    // Save the clone as a PNG
                    Image img = clone;
                    img.Save(ms, ImageFormat.Png);
                    sprites[i].PNGBytes = ms.ToArray();
                }

                bytes.AddRange(System.BitConverter.GetBytes(SpriteFileHeaderLocation + prevPNGBytesLength));
                bytes.AddRange(System.BitConverter.GetBytes(sprites[i].PNGBytes.Length + (108 + (12 * sprites[i].Rects.Count()))));
                prevPNGBytesLength += sprites[i].PNGBytes.Length + (108 + (12 * sprites[i].Rects.Count()));
            }

            foreach (Sprite sprite in sprites)
            {
                bytes.AddRange(Encoding.ASCII.GetBytes("<Sprite File Header>").ToArray());
                bytes.AddRange(new byte[80]);
                bytes.AddRange(System.BitConverter.GetBytes(sprite.Rects.Count()));
                foreach (SpriteRect rect in sprite.Rects)
                {
                    bytes.AddRange(System.BitConverter.GetBytes(rect.x).ToArray());
                    bytes.AddRange(System.BitConverter.GetBytes(rect.y).ToArray());
                    bytes.AddRange(System.BitConverter.GetBytes(rect.width).ToArray());
                    bytes.AddRange(System.BitConverter.GetBytes(rect.height).ToArray());
                    bytes.AddRange(System.BitConverter.GetBytes(rect.pivx).ToArray());
                    bytes.AddRange(System.BitConverter.GetBytes(rect.pivy).ToArray());
                }
                bytes.AddRange(new byte[4]);

                bytes.AddRange(sprite.PNGBytes);
            }
            return bytes.ToArray();
        }

        private void RectangleDataView_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.DrawString("X:", new Font("Century Gothic", 9.0f, FontStyle.Bold), Brushes.Black, new Point(50, 26));
            g.DrawString("Y:", new Font("Century Gothic", 9.0f, FontStyle.Bold), Brushes.Black, new Point(50, 76));
            g.DrawString("Width:", new Font("Century Gothic", 9.0f, FontStyle.Bold), Brushes.Black, new Point(250, 26));
            g.DrawString("Height:", new Font("Century Gothic", 9.0f, FontStyle.Bold), Brushes.Black, new Point(250, 76));
            g.DrawString("Pivot X:", new Font("Century Gothic", 9.0f, FontStyle.Bold), Brushes.Black, new Point(435, 26));
            g.DrawString("Pivot Y:", new Font("Century Gothic", 9.0f, FontStyle.Bold), Brushes.Black, new Point(435, 76));
        }

        private void PictureView_Paint(object? sender, PaintEventArgs e)
        {
            if (SelectedSpriteImage == null) return;
            if (SelectedSprite == null) return;

            PictureView.Height = SelectedSpriteImage.Height;
            PictureView.Width = SelectedSpriteImage.Width;

            float x = (PictureView.Width - SelectedSpriteImage.Width) / 2;
            float y = (PictureView.Height - SelectedSpriteImage.Height) / 2;

            // Adjust compositing quality only if necessary.
            e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

            // Fill background and draw the sprite
            //e.Graphics.FillRectangle(Brushes.LightGray, x, y, SelectedSpriteImage.Width, SelectedSpriteImage.Height);
            e.Graphics.DrawImageUnscaled(SelectedSpriteImage!, (int)x, (int)y);

            // Cache the selected parent node and its rects;
            var rects = SelectedSprite.Rects.AsParallel().Select(r => r.ToRectangle()).ToList();
            var selectedRect = SelectedRect?.ToRectangle();

            // Use a for loop for better performance
            for (int i = 0; i < rects.Count; i++)
            {
                var r = rects[i];
                Pen pen = yellowPen;

                if (SelectedRectIndex >= 0 && selectedRect.Equals(r))
                {
                    pen = redPen;
                }

                e.Graphics.DrawRectangle(pen, new Rectangle(r.X + (int)x, r.Y + (int)y, r.Width, r.Height));
            }

            if (IsRemovingBackground)
            {
                int gridSize = 3; // 3x3 grid
                int cellWidth = SelectedPixelRect.Width / gridSize;
                int cellHeight = SelectedPixelRect.Height / gridSize;

                // Adjust the SelectedPixelRect to fit the exact grid size
                SelectedPixelRect.Width = cellWidth * gridSize;
                SelectedPixelRect.Height = cellHeight * gridSize;

                int pixelIndex = 0;

                // Loop through the grid (3 rows, 3 columns)
                for (int row = 0; row < gridSize; row++)
                {
                    for (int col = 0; col < gridSize; col++)
                    {
                        if (pixelIndex >= SelectedPixels.Count) break;

                        // Calculate the position for the current grid cell
                        int cellX = SelectedPixelRect.X + (col * cellWidth);
                        int cellY = SelectedPixelRect.Y + (row * cellHeight);
                        Rectangle cellRect = new Rectangle(cellX, cellY, cellWidth, cellHeight);

                        // Get the corresponding pixel color
                        var sp = SelectedPixels[pixelIndex++];

                        using (SolidBrush b = new SolidBrush(sp))
                        {
                            // Fill the rectangle for this cell with the pixel color
                            e.Graphics.FillRectangle(b, cellRect);
                        }
                    }
                }

                // Draw the adjusted outer border rectangle
                e.Graphics.DrawRectangle(Pens.Black, SelectedPixelRect);
            }

        }

        private void SetRectangleTextBox(string ControlName, string text)
        {
            if (RectangleDataView.Controls.Find(ControlName, true).First() is TextBox txtBox)
            {
                txtBox.Text = text;
            }
        }

        private void SpriteList_NodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenuStrip cm = new ContextMenuStrip();
                if (e.Node.Parent == null)
                {
                    int SpriteIndex = e.Node.Index;
                    {   // Add rectangle
                        ToolStripMenuItem addRect = new ToolStripMenuItem("Add Rectangle");
                        cm.Items.Add(addRect);
                        addRect.Click += (send, EventArg) => { addRectangle_Click(send, SpriteIndex, e.Node.Nodes.Count - 1); };
                    }
                    {
                        ToolStripMenuItem removeBackground = new ToolStripMenuItem("Remove Background");
                        cm.Items.Add(removeBackground);
                        removeBackground.Click += (send, args) => { removeBackground_Click(SpriteIndex); };
                    }
                    {   // Replace PNG
                        ToolStripMenuItem replacePNG = new ToolStripMenuItem("Replace PNG");
                        cm.Items.Add(replacePNG);
                        replacePNG.Click += (send, EventArgs) => { replaceSprite_Click(send, SpriteIndex); };
                    }
                    {   // Save PNG
                        ToolStripMenuItem savePNG = new ToolStripMenuItem("Save PNG");
                        cm.Items.Add(savePNG);
                        savePNG.Click += (send, EventArgs) => { SavePNG_Click(send, SpriteIndex); };
                    }
                    {   // Delete PNG
                        ToolStripMenuItem deleteSprite = new ToolStripMenuItem("Delete Sprite");
                        cm.Items.Add(deleteSprite);
                        deleteSprite.Click += (send, EventArgs) => { deleteSprite_Click(SpriteIndex); };
                    }
                    {   // Show Animation
                        ToolStripMenuItem showAnimation = new ToolStripMenuItem("Show Animation");
                        cm.Items.Add(showAnimation);
                        //showAnimation.Click += (send, EventArgs) => { showAnimation_Click(send, EventArgs); };
                    }
                }
                else
                {
                    int SpriteIndex = e.Node.Parent.Index;
                    int SpriteChildIndex = e.Node.Index;
                    ToolStripMenuItem deleteChildNode = new ToolStripMenuItem("Remove Rectangle");
                    cm.Items.Add(deleteChildNode);
                    deleteChildNode.Click += (send, EventArgs) => { deleteChildNode_Click(send, SpriteIndex, SpriteChildIndex); };

                }
                SpriteList.ContextMenuStrip = cm;
            }
        }

        private void deleteSprite_Click(int spriteIndex)
        {
            if (SelectedSprite == null || SelectedParentNode == -1)
                return;

            // Suspend the layout to avoid UI updates during modification
            SpriteList.BeginUpdate();

            // Remove the sprite from both the list and the tree nodes
            SpriteList.Nodes.RemoveAt(spriteIndex);
            sprites.RemoveAt(spriteIndex);

            // Update both the tree node text and sprite ID in one go
            for (int i = 0; i < SpriteList.Nodes.Count; i++)
            {
                SpriteList.Nodes[i].Text = $"Sprite {i}";
                sprites[i].ID = i;
            }

            // Resume layout and trigger a single redraw
            SpriteList.EndUpdate();
        }

        private void addRectangle_Click(object? send, int spriteIndex, int v)
        {
            if (SelectedSprite == null)
                return;

            if (SelectedParentNode == -1)
                return;

            var nodeList = SpriteList.Nodes[SelectedSprite.ID];

            sprites.First(x => x.ID == SelectedSprite.ID).Rects.Add(new SpriteRect());
            nodeList.Nodes.Add(nodeList.Nodes.Count.ToString());
        }

        private void deleteChildNode_Click(object? send, int spriteIndex, int spriteChildIndex)
        {
            SpriteList.Nodes[spriteIndex].Nodes.RemoveAt(spriteChildIndex);

            int newIndex = 0;
            foreach (TreeNode node in SpriteList.Nodes[spriteIndex].Nodes)
            {
                node.Text = newIndex.ToString();
                newIndex++;
            }

            List<SpriteRect> newRects = sprites[spriteIndex].Rects.ToList();
            newRects.RemoveAt(spriteChildIndex);
            sprites[spriteIndex].Rects = newRects;
            PictureView.Refresh();
        }

        private void replaceSprite_Click(object? send, int spriteIndex)
        {
            if (sprites.FirstOrDefault(x => x.ID == SelectedParentNode) is Sprite sprite)
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = string.Join('|', [
                        ".png Portable Network Graphics File (*.png)|*.png",
                        ".bmp Bitmap File (*.bmp)|*.bmp"
                        ]);

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        sprite.bitmap = new Bitmap(Image.FromFile(ofd.FileName));
                        using (MemoryStream ms = new MemoryStream())
                        {
                            switch (Path.GetExtension(ofd.FileName).ToLower())
                            {
                                case ".png":
                                    sprite.bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                    sprite.PNGBytes = ms.ToArray();
                                    break;
                                case ".bmp":
                                    sprite.bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                    sprite.PNGBytes = ms.ToArray();
                                    break;
                            }
                            ms.Close();
                        }
                        SelectedSpriteImage = sprite.bitmap;
                        PictureView.Refresh();
                    }
                }
            }
        }

        private void SavePNG_Click(object? sender, int spriteIndex)
        {
            if (SelectedSpriteImage == null)
                return;

            if (sprites[SelectedParentNode].bitmap == null)
            {
                sprites[SelectedParentNode].bitmap = new Bitmap(Image.FromStream(new MemoryStream(sprites[SelectedParentNode].PNGBytes)));
                if (sprites[SelectedParentNode].bitmap == null)
                    return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = string.Join('|', [
                    ".png Portable Network Graphics File (*.png)|*.png",
                    ".bmp Bitmap File (*.bmp)|*.bmp"
                    ]);

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    switch (Path.GetExtension(sfd.FileName).ToLower())
                    {
                        case ".png":
                            sprites[SelectedParentNode].bitmap!.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                            break;
                        case ".bmp":
                            sprites[SelectedParentNode].bitmap!.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                            break;
                    }
                }
            }
        }

        private void SpriteList_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            SelectedRectIndex = -1;
            SelectedParentNode = -1;
            SelectedRect = null;
            SelectedSprite = null;

            if (SpriteList.SelectedNode.Parent == null)
            {
                SelectedParentNode = SpriteList.SelectedNode.Index;
            }
            else
            {
                SelectedParentNode = SpriteList.SelectedNode.Parent.Index;
                SelectedRectIndex = SpriteList.SelectedNode.Index;
            }

            if (sender is TreeView tv && tv.Parent?.Controls.Count > 0 && tv.Parent?.Controls["RectangleDataView"] is Panel rdvPanel && rdvPanel.Controls.Count > 0
                && rdvPanel.Controls["txtBox_X"] is TextBox txtX && rdvPanel.Controls["txtBox_Y"] is TextBox txtY
                && rdvPanel.Controls["txtBox_W"] is TextBox txtWidth && rdvPanel.Controls["txtBox_H"] is TextBox txtHeight
                && rdvPanel.Controls["txtBox_PivX"] is TextBox txtPivX && rdvPanel.Controls["txtBox_PivY"] is TextBox txtPivY
                && sprites.FirstOrDefault(x => x.ID == SelectedParentNode) is Sprite sprite)
            {
                // Remove existing event handlers
                txtX.TextChanged -= TxtChangedHandler;
                txtY.TextChanged -= TxtChangedHandler;
                txtWidth.TextChanged -= TxtChangedHandler;
                txtHeight.TextChanged -= TxtChangedHandler;
                txtPivX.TextChanged -= TxtChangedHandler;
                txtPivY.TextChanged -= TxtChangedHandler;

                txtX.Enabled = SelectedRectIndex != -1;
                txtY.Enabled = SelectedRectIndex != -1;
                txtWidth.Enabled = SelectedRectIndex != -1;
                txtHeight.Enabled = SelectedRectIndex != -1;
                txtPivX.Enabled = SelectedRectIndex != -1;
                txtPivY.Enabled = SelectedRectIndex != -1;

                txtX.Text = "";
                txtY.Text = "";
                txtWidth.Text = "";
                txtHeight.Text = "";
                txtPivX.Text = "";
                txtPivY.Text = "";

                if (SelectedRectIndex != -1 && sprite.Rects.ElementAtOrDefault(SelectedRectIndex) is SpriteRect rect)
                {
                    txtX.Text = rect.x.ToString();
                    txtY.Text = rect.y.ToString();
                    txtWidth.Text = rect.width.ToString();
                    txtHeight.Text = rect.height.ToString();
                    txtPivX.Text = rect.pivx.ToString();
                    txtPivY.Text = rect.pivy.ToString();

                    // Add a common event handler for text changes
                    txtX.TextChanged += TxtChangedHandler;
                    txtY.TextChanged += TxtChangedHandler;
                    txtWidth.TextChanged += TxtChangedHandler;
                    txtHeight.TextChanged += TxtChangedHandler;
                    txtPivX.TextChanged += TxtChangedHandler;
                    txtPivY.TextChanged += TxtChangedHandler;

                    SelectedRect = rect;
                }

                SelectedSprite = sprite;
                SelectedSpriteImage = sprite.bitmap;
                PictureView.Refresh();
            }
        }

        private void TxtChangedHandler(object? sender, EventArgs e)
        {
            if (SelectedRectIndex >= 0 && sender is TextBox txtBox && SelectedRect != null)
            {
                if (short.TryParse(txtBox.Text, out short value))
                {
                    switch (txtBox.Name)
                    {
                        case "txtBox_X":
                            SelectedRect.x = value;
                            break;
                        case "txtBox_Y":
                            SelectedRect.y = value;
                            break;
                        case "txtBox_W":
                            SelectedRect.width = value;
                            break;
                        case "txtBox_H":
                            SelectedRect.height = value;
                            break;
                        case "txtBox_PivX":
                            SelectedRect.pivx = value;
                            break;
                        case "txtBox_PivY":
                            SelectedRect.pivy = value;
                            break;
                    }
                    PictureView.Refresh();
                }
            }
        }

        public void OpenPAKFile(string fileName)
        {
            ClearTable();
            ToggleMenuItem(3, true);
            ToggleMenuItem(4, true);
            ToggleMenuItem(5, true);
            ToggleMenuItem(6, true);
            ToggleMenuItem(8, true);
            ToggleMenuItem(9, true);
            ToggleMenuItem(11, true);

            BitReader reader = new BitReader(fileName);

            if (reader.ReadString(17, 8) != "<Pak file header>")
                throw new InvalidDataException("Invalid PAK file, no header found!");
            reader.SeekBytes(reader.BytePosition + 3);
            int SpriteCount = reader.ReadInt32();
            for (int i = 0; i < SpriteCount; i++)
            {
                sprites.Add(new Sprite()
                {
                    ID = i
                });
                SpriteList.Nodes.Add("Sprite " + i);
            }

            for (int i = 0; i < sprites.Count; i++)
            {
                sprites[i].StartBytePosition = reader.ReadUInt32();
                sprites[i].EndBytePosition = reader.ReadUInt32();
            }

            if (reader.ReadString(20, 8) != "<Sprite File Header>")
                throw new InvalidDataException("Invalid or corrupt PAK file, no Sprite File Header found!");

            foreach (var sprite in sprites)
            {
                var nodeList = SpriteList.Nodes[sprite.ID];
                reader.SeekBytes(sprite.StartBytePosition + 100);
                sprite.FrameCount = reader.ReadInt32();
                sprite.Rects = new List<SpriteRect>();
                for (int i = 0; i < sprite.FrameCount; i++)
                {
                    nodeList.Nodes.Add(nodeList.GetNodeCount(false).ToString());
                    sprite.Rects.Add(new SpriteRect()
                    {
                        x = reader.ReadInt16(),
                        y = reader.ReadInt16(),
                        width = reader.ReadInt16(),
                        height = reader.ReadInt16(),
                        pivx = reader.ReadInt16(),
                        pivy = reader.ReadInt16()
                    });
                }
                sprite.BitmapStartBytePosition = (uint)(sprite.StartBytePosition + (108 + (12 * sprite.FrameCount)));
                reader.SeekBytes(sprite.BitmapStartBytePosition);
                sprite.PNGBytes = reader.ReadBytes(sprite.EndBytePosition - (uint)(108 + (12 * sprite.FrameCount))).ToArray();

                using (Image img = Image.FromStream(new MemoryStream(sprite.PNGBytes)))
                    sprite.bitmap = new Bitmap(img);
            }

            OpenFileLocation = fileName;

            this.Text = "PAK Editor - " + Path.GetFileName(fileName);
        }

        private void FileOpen_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = ".PAK Data Files (*.pak)|*.pak";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    OpenPAKFile(ofd.FileName);
                }
            }
        }

        private void ClearTable()
        {
            sprites.Clear();
            SpriteList.Nodes.Clear();
            SelectedRectIndex = 0;
            SelectedParentNode = 0;
            SelectedSpriteImage = null;
            SelectedSprite = null;
            SelectedRect = null;
            ToggleMenuItem(3, false);
            ToggleMenuItem(4, false);
            ToggleMenuItem(5, false);
            ToggleMenuItem(6, false);
            ToggleMenuItem(8, false);
            ToggleMenuItem(9, false);
            ToggleMenuItem(11, false);
            PictureView.Refresh();
            GC.Collect();
        }

        private void ToggleMenuItem(int Index, bool enable)
        {
            ToolStripItem _tmp = file.DropDownItems[Index];
            _tmp.Enabled = enable;
            file.DropDownItems.RemoveAt(Index);
            file.DropDownItems.Insert(Index, _tmp);
        }

        private MouseEventHandler? pictureView_MouseClickHandler = null;
        private MouseEventHandler? pictureView_MouseMoveHandler = null;

        private void PictureView_MouseClick(object? sender, MouseEventArgs e, int spriteIndex)
        {
            // Get the pixel color under the mouse and do something with it
            Color c = GetPixelColorUnderMouse(e);
            if(c != Color.Empty)
            {
                Point pixelCoord = GetPixelCoordinates();
                DialogResult dr = MessageBox.Show($"Color under mouse at ({pixelCoord.X}, {pixelCoord.Y}): {c}", "Should remove?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    DoRemoveBackground(c, spriteIndex);
                }
            } else
            {
                MessageBox.Show("Selected Pixel outside the bounds of the image");
            }

            // Unsubscribe from the event after the click
            PictureView.MouseClick -= pictureView_MouseClickHandler;
            PictureView.MouseMove -= pictureView_MouseMoveHandler;
            Cursor = Cursors.Default;
            IsRemovingBackground = false;
            PictureView.Refresh();
        }

        private void removeBackground_Click(int spriteIndex, bool IsRemovingAll = false)
        {
            Cursor = Cursors.Cross;

            // Assign the event handler
            if (!IsRemovingAll)
                pictureView_MouseClickHandler = (sender, e) => PictureView_MouseClick(sender, e, spriteIndex);

            pictureView_MouseMoveHandler = (sender, e) => PictureView_MouseMove(sender, e, spriteIndex);

            // Subscribe to the event
            if(!IsRemovingAll)
                PictureView.MouseClick += pictureView_MouseClickHandler;

            PictureView.MouseMove += pictureView_MouseMoveHandler;

            IsRemovingBackground = true;
        }

        private void PictureView_MouseMove(object? sender, MouseEventArgs e, int spriteIndex)
        {
            SelectedPixels.Clear();

            // Capture the top-left corner of the selected rectangle
            Point ptc = PointToClient(Cursor.Position);
            SelectedPixelRect = new Rectangle(ptc.X - 305, ptc.Y - 80, 50, 50);

            // Define the relative offsets for the 3x3 grid
            int[,] offsets = new int[,]
            {
                { -1, -1 }, { 0, -1 }, { 1, -1 }, // Top row
                { -1, 0 },  { 0, 0 },  { 1, 0 },  // Middle row (center pixel is the origin)
                { -1, 1 },  { 0, 1 },  { 1, 1 }   // Bottom row
            };

            // Loop through each of the 9 pixels in the grid
            for (int i = 0; i < 9; i++)
            {
                // Get the x and y offset for each pixel
                int offsetX = offsets[i, 0];
                int offsetY = offsets[i, 1];

                // If it's the center pixel (origin), use the exact cursor position
                if (offsetX == 0 && offsetY == 0)
                {
                    SelectedPixels.Add(GetPixelColorUnderMouse(e)); // Origin pixel
                }
                else
                {
                    // Adjust the coordinates to get neighboring pixels
                    MouseEventArgs pixelEvent = new MouseEventArgs(
                        e.Button,
                        e.Clicks,
                        e.X + offsetX,
                        e.Y + offsetY,
                        e.Delta
                    );

                    // Get the color at this specific pixel and add it to the list
                    SelectedPixels.Add(GetPixelColorUnderMouse(pixelEvent));
                }
            }

            // Refresh the view to update the drawn rectangle and pixels
            PictureView.Refresh();
        }


        private void DoRemoveBackground(Color pixelTest, int spriteIndex)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Bitmap clone = new Bitmap(sprites[spriteIndex].bitmap!.Width, sprites[spriteIndex].bitmap!.Height, PixelFormat.Format32bppArgb);

                // Lock the original bitmap's bits
                BitmapData originalData = sprites[spriteIndex].bitmap!.LockBits(
                    new Rectangle(0, 0, sprites[spriteIndex].bitmap!.Width, sprites[spriteIndex].bitmap!.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                // Lock the clone bitmap's bits
                BitmapData cloneData = clone.LockBits(
                    new Rectangle(0, 0, clone.Width, clone.Height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb);

                // Calculate the size of the data
                int _bytes = Math.Abs(originalData.Stride) * originalData.Height;

                // Create a buffer to hold the pixel data
                byte[] pixelBuffer = new byte[_bytes];

                // Copy the data from the original bitmap to the buffer
                Marshal.Copy(originalData.Scan0, pixelBuffer, 0, _bytes);

                // Define the color to remove (e.g., pure red with full alpha)
                byte targetA = pixelTest.A; // Alpha
                byte targetR = pixelTest.R; // Red
                byte targetG = pixelTest.G;   // Green
                byte targetB = pixelTest.B;   // Blue

                // Iterate over each pixel (32 bits per pixel)
                for (int k = 0; k < pixelBuffer.Length; k += 4)
                {
                    byte b = pixelBuffer[k];
                    byte g = pixelBuffer[k + 1];
                    byte r = pixelBuffer[k + 2];
                    byte a = pixelBuffer[k + 3];

                    // Check if the pixel matches the target color
                    if (a == targetA && r == targetR && g == targetG && b == targetB)
                    {
                        // Set alpha to 0 (transparent)
                        pixelBuffer[k + 3] = 0;
                    }
                }

                // Copy the modified data from the buffer to the clone bitmap
                Marshal.Copy(pixelBuffer, 0, cloneData.Scan0, _bytes);

                // Unlock the bits
                sprites[spriteIndex].bitmap!.UnlockBits(originalData);
                clone.UnlockBits(cloneData);

                // Save the clone as a PNG
                Image img = clone;
                img.Save(ms, ImageFormat.Png);
                sprites[spriteIndex].PNGBytes = ms.ToArray();
                sprites[spriteIndex].bitmap = clone;
                SelectedSprite = sprites[spriteIndex];
                SelectedSpriteImage = SelectedSprite.bitmap;
                PictureView.Refresh();
            }
        }

        public Point GetPixelCoordinates()
        {
            using (Bitmap bmp = new Bitmap(SelectedSpriteImage!))
            {
                // Calculate the x and y location of the image within the PictureView panel
                float xOffset = (PictureView.Width - bmp.Width) / 2;
                float yOffset = (PictureView.Height - bmp.Height) / 2;

                // Check if the mouse click is within the bounds of the image
                if (Cursor.Position.X >= xOffset && Cursor.Position.X < xOffset + bmp.Width &&
                    Cursor.Position.Y >= yOffset && Cursor.Position.Y < yOffset + bmp.Height)
                {
                    // Map the mouse coordinates to the image's pixel coordinates
                    int imageX = (int)(Cursor.Position.X - xOffset);
                    int imageY = (int)(Cursor.Position.Y - yOffset);

                    // Get the color of the pixel at the mapped coordinates
                    return new Point(imageX, imageY);
                }
                else
                {
                    return Point.Empty;
                }
            }
        }

        public Color GetPixelColorUnderMouse(MouseEventArgs e)
        {
            using (Bitmap bmp = new Bitmap(SelectedSpriteImage!))
            {
                // Calculate the x and y location of the image within the PictureView panel
                float xOffset = (PictureView.Width - bmp.Width) / 2;
                float yOffset = (PictureView.Height - bmp.Height) / 2;

                // Check if the mouse click is within the bounds of the image
                if (e.X >= xOffset && e.X < xOffset + bmp.Width &&
                    e.Y >= yOffset && e.Y < yOffset + bmp.Height)
                {
                    // Map the mouse coordinates to the image's pixel coordinates
                    int imageX = (int)(e.X - xOffset);
                    int imageY = (int)(e.Y - yOffset);

                    // Get the color of the pixel at the mapped coordinates
                    Color color = bmp.GetPixel(imageX, imageY);
                    return color;
                }
                else
                {
                    return Color.Empty;
                }
            }
        }

        public class SpriteRect
        {
            public short x;
            public short y;
            public short width;
            public short height;
            public short pivx;
            public short pivy;

            public Rectangle ToRectangle()
            {
                return new Rectangle(x, y, width, height);
            }
        };

        public class Sprite
        {
            public int ID { get; set; } = -1;
            public uint StartBytePosition { get; set; } = 0;
            public uint EndBytePosition { get; set; } = 0;
            public int FrameCount { get; set; } = -1;
            public List<SpriteRect> Rects { get; set; } = new List<SpriteRect>();
            public uint BitmapStartBytePosition { get; set; } = 0;
            public byte[] PNGBytes { get; set; } = [];
            public Bitmap? bitmap { get; set; } = null;
        }
    }
}
