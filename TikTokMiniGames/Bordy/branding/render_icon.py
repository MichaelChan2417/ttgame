from PIL import Image, ImageDraw, ImageFilter

OUT = 1024
S = OUT * 3                      # supersample for crisp edges
f = S / 1024.0
def u(v): return int(round(v * f))

# --- background: diagonal-ish vertical gradient (indigo night) ---
top = (58, 63, 102); bot = (35, 39, 63)
bg = Image.new("RGB", (S, S))
px = bg.load()
for y in range(S):
    t = y / (S - 1)
    c = tuple(int(top[i] + (bot[i] - top[i]) * t) for i in range(3))
    for x in range(S):
        px[x, y] = c
img = bg.convert("RGBA")

# --- subtle puzzle grid ---
grid = Image.new("RGBA", (S, S), (0, 0, 0, 0))
gd = ImageDraw.Draw(grid)
for gx in (256, 512, 768):
    gd.line([(u(gx), u(40)), (u(gx), u(984))], fill=(255, 255, 255, 22), width=u(4))
for gy in (256, 512, 768):
    gd.line([(u(40), u(gy)), (u(984), u(gy))], fill=(255, 255, 255, 22), width=u(4))
img = Image.alpha_composite(img, grid)

def shadow(cx, cy, r):
    lay = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(lay)
    d.ellipse([u(cx-r), u(cy-r*0.9)+u(24), u(cx+r), u(cy+r*0.9)+u(24)], fill=(0, 0, 0, 90))
    lay = lay.filter(ImageFilter.GaussianBlur(u(26)))
    return lay

# --- SUN (top-left) ---
scx, scy, sr = 372, 372, 132
img = Image.alpha_composite(img, shadow(scx, scy, sr))
# rays
for ang in range(0, 360, 45):
    ray = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    rd = ImageDraw.Draw(ray)
    rw = u(26)
    rd.rounded_rectangle([u(scx)-rw//2, u(scy-182), u(scx)+rw//2, u(scy-130)],
                         radius=rw//2, fill=(255, 173, 40, 255))
    ray = ray.rotate(ang, center=(u(scx), u(scy)), resample=Image.BICUBIC)
    img = Image.alpha_composite(img, ray)
sun = Image.new("RGBA", (S, S), (0, 0, 0, 0))
sd = ImageDraw.Draw(sun)
sd.ellipse([u(scx-sr), u(scy-sr), u(scx+sr), u(scy+sr)], fill=(255, 158, 28, 255))
sd.ellipse([u(scx-sr*0.72), u(scy-sr*0.82), u(scx+sr*0.72), u(scy+sr*0.62)], fill=(255, 206, 100, 255))
img = Image.alpha_composite(img, sun)

# --- MOON (bottom-right), crescent via alpha erase ---
mcx, mcy, mr = 662, 662, 140
img = Image.alpha_composite(img, shadow(mcx, mcy, mr))
moon = Image.new("RGBA", (S, S), (0, 0, 0, 0))
md = ImageDraw.Draw(moon)
md.ellipse([u(mcx-mr), u(mcy-mr), u(mcx+mr), u(mcy+mr)], fill=(214, 228, 255, 255))
md.ellipse([u(mcx-mr*0.7), u(mcy-mr*0.86), u(mcx+mr*0.55), u(mcy+mr*0.5)], fill=(240, 245, 255, 255))
# erase offset circle to carve the crescent
oc = (mcx + 74, mcy - 46, 126)
md.ellipse([u(oc[0]-oc[2]), u(oc[1]-oc[2]), u(oc[0]+oc[2]), u(oc[1]+oc[2])], fill=(0, 0, 0, 0))
img = Image.alpha_composite(img, moon)

# downscale
img = img.convert("RGB").resize((OUT, OUT), Image.LANCZOS)
img.save("bordy-icon-1024.png")
print("saved bordy-icon-1024.png", img.size)
