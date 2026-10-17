using System.Windows.Input;using System.Windows.Media.Animation;using System.Windows;using System.Windows.Controls;using System.Windows.Media;using System.Windows.Media.Imaging;using System.Windows.Media.Media3D;
namespace OrbitLauncher;
public sealed class SkinPreview:Viewport3D {
 readonly Dictionary<string,AxisAngleRotation3D> joints=new();readonly TranslateTransform3D bob=new();
 readonly System.Windows.Threading.DispatcherTimer emoteTimer=new(){Interval=TimeSpan.FromSeconds(2.5)};
 internal bool HasPoseAnimation=>joints.Values.Any(j=>j.HasAnimatedProperties)||bob.HasAnimatedProperties;
 internal double RightArmAngle=>joints.TryGetValue("rightArm",out var arm)?arm.Angle:0;
 public double Yaw=>rotation.Angle;public string ActiveEmote{get;private set;}="";
 public void DragBy(double pixels){rotation.Angle=(rotation.Angle+pixels*.65)%360;}
 public void StopEmote(){emoteTimer.Stop();foreach(var joint in joints.Values){joint.BeginAnimation(AxisAngleRotation3D.AngleProperty,null);joint.Angle=0;}bob.BeginAnimation(TranslateTransform3D.OffsetYProperty,null);bob.OffsetY=0;ActiveEmote="";}
 static DoubleAnimationUsingKeyFrames Motion(params double[] angles){var animation=new DoubleAnimationUsingKeyFrames{Duration=TimeSpan.FromSeconds(2.4),FillBehavior=FillBehavior.Stop};for(int i=0;i<angles.Length;i++)animation.KeyFrames.Add(new EasingDoubleKeyFrame(angles[i],KeyTime.FromTimeSpan(TimeSpan.FromSeconds(i*2.4/(angles.Length-1)))){EasingFunction=new SineEase{EasingMode=EasingMode.EaseInOut}});return animation;}
 public void PlayEmote(string name,bool reduced=false){
  StopEmote();if(reduced||!IsVisible)return;ActiveEmote=name;emoteTimer.Start();
  void Joint(string key,params double[] values){if(joints.TryGetValue(key,out var joint))joint.BeginAnimation(AxisAngleRotation3D.AngleProperty,Motion(values));}
  if(name=="wave"){Joint("rightArm",0,-145,-120,-150,-120,-145,0);Joint("head",0,-10,0,-10,0);}
  else if(name=="dance"){Joint("rightArm",0,-55,35,-55,35,0);Joint("leftArm",0,55,-35,55,-35,0);Joint("rightLeg",0,25,-25,25,-25,0);Joint("leftLeg",0,-25,25,-25,25,0);bob.BeginAnimation(TranslateTransform3D.OffsetYProperty,Motion(0,1,0,1,0,1,0));}
  else if(name=="cheer"){Joint("rightArm",0,-150,-130,-150,0);Joint("leftArm",0,150,130,150,0);bob.BeginAnimation(TranslateTransform3D.OffsetYProperty,Motion(0,2,0,2,0));}
  else ActiveEmote="";
 }

 readonly ModelVisual3D visual=new();readonly AxisAngleRotation3D rotation=new(new Vector3D(0,1,0),-16);public SkinPreview(){emoteTimer.Tick+=(s,e)=>StopEmote();IsVisibleChanged+=(s,e)=>{if(!IsVisible)StopEmote();};Camera=new PerspectiveCamera(new Point3D(48,29,93),new Vector3D(-48,-13,-93),new Vector3D(0,1,0),40);Children.Add(visual);SizeChanged+=(s,e)=>Fit();RenderOptions.SetBitmapScalingMode(this,BitmapScalingMode.NearestNeighbor);Draw(null,false);}
 void Fit(){if(ActualHeight<1||Camera is not PerspectiveCamera camera)return;double distance=20*Math.Max(1,ActualWidth/ActualHeight)/Math.Tan(20*Math.PI/180);camera.Position=new Point3D(.46*distance,16+.13*distance,.88*distance);camera.LookDirection=new Vector3D(-.46*distance,-.13*distance,-.88*distance);}
 public void Turn(double amount){rotation.Angle+=amount;}
 public void Draw(BitmapSource? image,bool slim){if(image!=null)image=SkinTextures.Normalize(image);if(image?.PixelHeight==32)slim=false;var models=new Model3DGroup();models.Children.Add(new AmbientLight(Color.FromRgb(190,190,190)));models.Children.Add(new DirectionalLight(Colors.White,new Vector3D(-1,-1,-2)));StopEmote();var rootTransform=new Transform3DGroup();rootTransform.Children.Add(new RotateTransform3D(rotation,new Point3D(0,16,0)));rootTransform.Children.Add(bob);var body=new Model3DGroup{Transform=rootTransform};
 joints.Clear();var parts=new Dictionary<string,Model3DGroup>();var outerParts=new Dictionary<string,Model3DGroup>();var opaqueRoot=new Model3DGroup();var outerRoot=new Model3DGroup();body.Children.Add(opaqueRoot);body.Children.Add(outerRoot);
 void Part(string key,Vector3D axis,Point3D pivot){var angle=new AxisAngleRotation3D(axis,0);joints[key]=angle;var group=new Model3DGroup{Transform=new RotateTransform3D(angle,pivot)};parts[key]=group;opaqueRoot.Children.Add(group);var outer=new Model3DGroup{Transform=group.Transform};outerParts[key]=outer;outerRoot.Children.Add(outer);}
 Part("head",new Vector3D(1,0,0),new Point3D(0,24,0));Part("torso",new Vector3D(0,1,0),new Point3D(0,18,0));Part("rightArm",new Vector3D(0,0,1),new Point3D(-4,24,0));Part("leftArm",new Vector3D(0,0,1),new Point3D(4,24,0));Part("rightLeg",new Vector3D(1,0,0),new Point3D(-2,12,0));Part("leftLeg",new Vector3D(1,0,0),new Point3D(2,12,0));var brush=image==null?(Brush)new SolidColorBrush(Color.FromRgb(86,125,104)):new ImageBrush(image){ViewportUnits=BrushMappingMode.RelativeToBoundingBox,Stretch=Stretch.Fill};var material=new DiffuseMaterial(brush);int texH=image?.PixelHeight??64;
 void Box(double cx,double cy,double cz,double w,double h,double d,int u,int v,double extra=0,string limb="torso"){double a=cx-w/2-extra,b=cx+w/2+extra,lo=cy-h/2-extra,hi=cy+h/2+extra,back=cz-d/2-extra,front=cz+d/2+extra;
 void Face(Point3D[] points,double x,double y,double tw,double th){
  bool mirror=image?.PixelHeight==32&&limb is "leftArm" or "leftLeg";
  Material faceMaterial=material;byte[]? source=null;int cw=(int)tw,ch=(int)th;
  if(image!=null){
   var cropped=new CroppedBitmap(image,new Int32Rect((int)x,(int)y,cw,ch));cropped.Freeze();var format=new FormatConvertedBitmap(cropped,PixelFormats.Bgra32,null,0);source=new byte[cw*ch*4];format.CopyPixels(source,cw*4,0);
   const int scale=8;byte[] pixels=new byte[cw*ch*4*scale*scale];for(int py=0;py<ch*scale;py++)for(int px=0;px<cw*scale;px++)System.Buffer.BlockCopy(source,((py/scale)*cw+px/scale)*4,pixels,(py*cw*scale+px)*4,4);
   var texture=BitmapSource.Create(cw*scale,ch*scale,96,96,PixelFormats.Bgra32,null,pixels,cw*scale*4);texture.Freeze();var faceBrush=new ImageBrush(texture);RenderOptions.SetBitmapScalingMode(faceBrush,BitmapScalingMode.NearestNeighbor);faceMaterial=new DiffuseMaterial(faceBrush);
  }
  var mesh=new MeshGeometry3D();
  Point3D Position(double u,double v)=>points[0]+(points[3]-points[0])*u+(points[1]-points[0])*v;
  void Quad(double u0,double v0,double u1,double v1){
   int first=mesh.Positions.Count;mesh.Positions.Add(Position(u0,v0));mesh.Positions.Add(Position(u0,v1));mesh.Positions.Add(Position(u1,v1));mesh.Positions.Add(Position(u1,v0));
   mesh.TextureCoordinates.Add(new Point(mirror?1-u0:u0,v0));mesh.TextureCoordinates.Add(new Point(mirror?1-u0:u0,v1));mesh.TextureCoordinates.Add(new Point(mirror?1-u1:u1,v1));mesh.TextureCoordinates.Add(new Point(mirror?1-u1:u1,v0));
   foreach(int i in new[]{0,1,2,0,2,3})mesh.TriangleIndices.Add(first+i);
  }
  // WPF writes depth even for a texture's zero-alpha pixels. Leave those pixels
  // out of the overlay geometry instead of drawing a transparent full-face quad.
  if(extra>0&&source!=null){
   for(int row=0;row<ch;row++){int col=0;while(col<cw){while(col<cw&&source[(row*cw+col)*4+3]==0)col++;int first=col;while(col<cw&&source[(row*cw+col)*4+3]>0)col++;if(col>first)Quad((double)first/cw,(double)row/ch,(double)col/cw,(double)(row+1)/ch);}}
  }else Quad(0,0,1,1);
  if(mesh.Positions.Count==0)return;mesh.Freeze();if(faceMaterial.CanFreeze)faceMaterial.Freeze();
  (extra>0?outerParts:parts)[limb].Children.Add(new GeometryModel3D(mesh,faceMaterial));
 }
 Face(new[]{new Point3D(a,hi,front),new Point3D(a,lo,front),new Point3D(b,lo,front),new Point3D(b,hi,front)},u+d,v+d,w,h);
 Face(new[]{new Point3D(b,hi,back),new Point3D(b,lo,back),new Point3D(a,lo,back),new Point3D(a,hi,back)},u+2*d+w,v+d,w,h);
 Face(new[]{new Point3D(a,hi,back),new Point3D(a,lo,back),new Point3D(a,lo,front),new Point3D(a,hi,front)},image?.PixelHeight==32&&limb is "leftArm" or "leftLeg"?u+d+w:u,v+d,d,h);
 Face(new[]{new Point3D(b,hi,front),new Point3D(b,lo,front),new Point3D(b,lo,back),new Point3D(b,hi,back)},image?.PixelHeight==32&&limb is "leftArm" or "leftLeg"?u:u+d+w,v+d,d,h);
 Face(new[]{new Point3D(a,hi,back),new Point3D(a,hi,front),new Point3D(b,hi,front),new Point3D(b,hi,back)},u+d,v,w,d);
 Face(new[]{new Point3D(a,lo,front),new Point3D(a,lo,back),new Point3D(b,lo,back),new Point3D(b,lo,front)},u+d+w,v,w,d);}
 double arm=slim?3:4;Box(0,28,0,8,8,8,0,0,limb:"head");Box(0,18,0,8,12,4,16,16);Box(-4-arm/2,18,0,arm,12,4,40,16,limb:"rightArm");Box(4+arm/2,18,0,arm,12,4,image?.PixelHeight==32?40:32,image?.PixelHeight==32?16:48,limb:"leftArm");Box(-2,6,0,4,12,4,0,16,limb:"rightLeg");Box(2,6,0,4,12,4,image?.PixelHeight==32?0:16,image?.PixelHeight==32?16:48,limb:"leftLeg");
 if(image!=null){Box(0,28,0,8,8,8,32,0,.5,"head");if(image.PixelHeight==64){Box(0,18,0,8,12,4,16,32,.25);Box(-4-arm/2,18,0,arm,12,4,40,32,.25,"rightArm");Box(4+arm/2,18,0,arm,12,4,48,48,.25,"leftArm");Box(-2,6,0,4,12,4,0,32,.25,"rightLeg");Box(2,6,0,4,12,4,0,48,.25,"leftLeg");}}
 models.Children.Add(body);visual.Content=models;}
}
