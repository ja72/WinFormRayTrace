# WinFormRayTrace
C# port of the tiny ray tracer described in https://github.com/ssloy/tinyraytracer/wiki as a WinForms project. 

Here are some interesting features:

 - It is WinForms desktop application that draws the graphics on a `PictureBox` by rendering a bitmap and assigning it to the UI.
 = Bitmap rendering uses `LockBits()` and unsafe poiners for fast processing.
 - Uses `System.Numerics` for SIMD Vectors 
 - Outer loop is `Parallel.For()` in release mode, and regular loop in debug mode.
 - Is a lot more OOP with the intent of creating classes to combine related code.
   - `Scene` - Contains all the lights and spheres as well as rendering related properties. All of the code for intersections and ray casting are included in this class. Finally, the rendering to a bitmap is done here, via the `FrameBuffer` object.
   - `FrameBuffer` - This is a pixel array of colors. Each ray writes to a single pixel in this array and then rendered into a bitmap using the `Render(PixelFormat format)` method.
   - `Camera` - This handles the pixel to screen coordinates. Pixel coordinates go from 0 to `Height` and `Width` and screen coordinates go from 0.0 to 1.0 in the scene.
   - `Light` - This contains all the color and attributes for each light in the scene.
   - `Material` - This contains all the color and attributes for each material assigned to objects in the scene.
   - `Ray` - This is basic ray with a vector origin and a vector direction.
   - `Sphere` - This is a basic sphere with a center, radius and material information.
  - Drawing on the UI is done asynchronously in order to be more responsive since each frame might take more than 500 milliseconds to render.
