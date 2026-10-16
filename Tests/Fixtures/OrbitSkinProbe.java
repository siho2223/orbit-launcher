import java.lang.reflect.*;import java.net.*;import java.util.*;import java.io.*;import java.security.*;
public final class OrbitSkinProbe {
 public static void main(String[] args)throws Exception{
  UUID uuid=UUID.fromString(args[0]);String nickname=args[1];Class<?> type=Class.forName("com.mojang.authlib.yggdrasil.YggdrasilAuthenticationService");Object auth=null;
  for(Constructor<?> ctor:type.getConstructors()){Class<?>[] p=ctor.getParameterTypes();if(p.length==2&&p[0]==java.net.Proxy.class&&p[1]==String.class)auth=ctor.newInstance(java.net.Proxy.NO_PROXY,"");else if(p.length==1&&p[0]==java.net.Proxy.class)auth=ctor.newInstance(java.net.Proxy.NO_PROXY);}
  if(auth==null)throw new Exception("Unsupported authlib constructor");
  // Minecraft 1.17 catches AuthenticationException here to use OfflineSocialInteractions.
  // Empty successful responses throw an uncaught NPE during game initialization instead.
  for(Method m:type.getMethods())if(m.getName().equals("createSocialInteractionsService")){
   try{m.invoke(auth,"0");throw new Exception("Local cosmetic service must not grant online privileges");}
   catch(InvocationTargetException e){
    Class<?> expected=Class.forName("com.mojang.authlib.exceptions.AuthenticationException");
    if(!expected.isInstance(e.getCause()))throw new Exception("Offline social initialization crashed",e.getCause());
    Object offline=Class.forName("com.mojang.authlib.minecraft.OfflineSocialInteractions").getConstructor().newInstance();
    if(!offline.getClass().getName().equals("com.mojang.authlib.minecraft.OfflineSocialInteractions"))throw new Exception("Incorrect fallback service");
    System.out.println("PASS vanilla offline social fallback after rejected account request");
   }
  }
Object service=type.getMethod("createMinecraftSessionService").invoke(auth);Class<?> gp=Class.forName("com.mojang.authlib.GameProfile");Object profile=gp.getConstructor(UUID.class,String.class).newInstance(uuid,nickname);boolean fetched=false;
  for(Method m:service.getClass().getMethods())if(m.getName().equals("fillProfileProperties")&&m.getParameterTypes().length==2){profile=m.invoke(service,profile,true);fetched=true;break;}
  if(!fetched)for(Method m:service.getClass().getMethods())if(m.getName().equals("fetchProfile")){Object result=m.getParameterTypes().length==2?m.invoke(service,uuid,true):m.invoke(service,uuid);if(result==null)throw new Exception("No profile result");profile=result.getClass().getMethod("profile").invoke(result);fetched=true;break;}
  if(!fetched)throw new Exception("No profile fetch method");Object textures=null;for(Method m:service.getClass().getMethods())if(m.getName().equals("getTextures")){textures=m.getParameterTypes().length==2?m.invoke(service,profile,true):m.invoke(service,profile);break;}
  Object skin=null;if(textures instanceof Map){for(Map.Entry<?,?> entry:((Map<?,?>)textures).entrySet())if(entry.getKey().toString().equals("SKIN"))skin=entry.getValue();}else if(textures!=null)skin=textures.getClass().getMethod("skin").invoke(textures);
  if(skin==null)throw new Exception("Authlib returned no skin");String url=(String)skin.getClass().getMethod("getUrl").invoke(skin);if(!url.startsWith("http://127.0.0.1:"))throw new Exception("Not a local texture");String model=(String)skin.getClass().getMethod("getMetadata",String.class).invoke(skin,"model");if(!args[2].equals(model))throw new Exception("Incorrect model: "+model);
  ByteArrayOutputStream out=new ByteArrayOutputStream();try(InputStream input=new URL(url).openStream()){byte[] b=new byte[4096];int n;while((n=input.read(b))!=-1)out.write(b,0,n);}StringBuilder digest=new StringBuilder();for(byte b:MessageDigest.getInstance("SHA-256").digest(out.toByteArray()))digest.append(String.format("%02x",b));if(!digest.toString().equals(args[3]))throw new Exception("Texture mismatch");System.out.println("PASS signed local skin, PNG bytes and "+model+" model through real authlib");
 }
}
