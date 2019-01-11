"""
2D Star Fox-esque game
Written by Jonathan Wang March 9 - May 22, 2017
Shoot your way through space to defeat the final boss!
Unfortunately, there wasn't enough time to add any rolls. Hence Star Fox-esque.

v1.0 finished May 22...that took a while.
v1.1 finished May 23. Love you Dad. Fixed 2 major bugs and some graphics.

DISCLAIMER: All names, images, and sprites were not created by me; however, all images and sprites that
appear in this game were edited in some way by me.

Documentation:
ID value is specific to each type of object:
1 is for player
10 - 99 is for enemies
2 - 9 is for bosses
100 - 499 is for round buildings
500 - 999 is for square buildings
<= -1 is for bullets; -1 is for player bullets
-9 - -2 is for boss bullets - boss bullet ID is the negative of the boss' ID
-10 is regular enemy bullets
1000+ is for rings

Each level will load a specific order of monsters and buildings, and a (technically) unique boss.
While in game, the background scrolls up at 5px/frame; all static buildings should move at same rate.
Polygon coordinate order is topL, bottomL, bottomR, topR.
~5 points given per enemy ship destroyed; ~1 point per building/obstacle; 150 points for defeating boss
or boss part; 10 points per ring.
Point values will vary depending on type of enemy.

The formula for starting health is 50 * ((level / 10.0) + 0.9), so at level 5 the starting health is 70.
30 health per second lost for hitting buildings, 60 health per second for enemies,
3 health each for bullets. If shield health reaches 0, game over.
Rings restore anywhere from 15 health to full heal; some increase max health.

Bullet colours:
#ffffff player bullets
#ff1111 normal enemy bullets
#11ff11 normal boss bullets
#ffff00 slowing bullets
#ff00ff shield depletion bullets
"""

import simplegui
import math, random

moving_left = False
moving_right = False
moving_up = False
moving_down = False
empty_list = False
WASD = True  # False = arrow keys to move, True = WASD
loading = False  # used to show text in between stage phases
boss_fight = False
boss_defeat = False

level = 0  # level 0 is main menu, 1+ is in-game levels, -1 and below is other menus
# -1 is level select, -2 is help, -3 is options, -4 is background story
level_completed = 0  # to track which levels to display
points = 0
level_time = 0  # timer for level (when time == 0, boss starts)
load_time = 0  # time in seconds to wait in order to display text between level phases
loading_timer = 0  # used to time load time
boss_text_time = 0  # time in seconds to wait in order to display boss' "speech"

ID_dict = {"player": 1, "fly": 10, "mosquito": 20, "mosquito1": 21, "mosquito2": 22,
           "hornet": 30, "queen_fly": 40, "queen_fly1": 41, "queen_fly2": 42, "mini_andross": 50,
           "asteroid": 100, "asteroid1": 101, "asteroid2": 102, "small_asteroid": 110, "small_asteroid1": 111,
           "debris": 120, "debris1": 121, "debris2": 122,
           "satellite": 500, "satellite1": 501, "satellite2": 502, "turret": 510, "turret1": 511, "turret2": 512,
           "ring": 1000, "ring1": 1001, "ring2": 1002, "yl_ring": 1010, "yl_ring1": 1011, "yl_ring2": 1012,
           "gr_ring": 1020, "gr_ring1": 1021, "gr_ring2": 1022, "re_ring": 1030, "re_ring1": 1031, "re_ring2": 1032}

enemy_list = []
building_list = []  # square buildings only
round_build_list = []  # round buildings only
bullet_list = []
ene_bullet_list = []
boss_bullet_list = []
wolf_pos = []  # position for wolf so pigma doesn't overlap wolf

# Loading images:
BACK_IMAGE = simplegui.load_image("http://i.imgur.com/flobOx8.jpg")
FOX_IMAGE = simplegui.load_image("http://i.imgur.com/sSnacEj.png")  # portrait of fox mccloud for background story
ARWING_IMAGE = simplegui.load_image("http://i.imgur.com/qJjQmFp.png")
ARWING_FRONT = simplegui.load_image("http://i.imgur.com/OSD6s4m.png")  # title screen image of arwing

# bosses
GRANGA_IMAGE = simplegui.load_image("http://i.imgur.com/eoOs0vk.png")
WOLFEN_IMAGE = simplegui.load_image("http://i.imgur.com/lFf4mzZ.png")
ANDROSS_HEAD = simplegui.load_image("http://i.imgur.com/tlB3UkN.png")
ANDROSS_LH = simplegui.load_image("http://i.imgur.com/MoiylaY.png")
ANDROSS_RH = simplegui.load_image("http://i.imgur.com/GK94FHe.png")

# enemies
FLY_IMG = simplegui.load_image("http://i.imgur.com/BZHaGx4.png")
MOSQUITO_IMG = simplegui.load_image("http://i.imgur.com/393qNav.png")
HORNET_IMG = simplegui.load_image("http://i.imgur.com/TqvGa6s.png")
QUEEN_IMG = simplegui.load_image("http://i.imgur.com/UX4kD9G.png")
MINI_ANDROSS_IMG = simplegui.load_image("http://i.imgur.com/ftXrPi5.png")

# buildings (both round and square; rings don't use an image)
SAT1 = simplegui.load_image("http://i.imgur.com/kS1LG1D.png")
SAT2 = simplegui.load_image("http://i.imgur.com/bUB23k5.png")
SAT3 = simplegui.load_image("http://i.imgur.com/nzgAyte.png")
ASTER1 = simplegui.load_image("http://i.imgur.com/a2Sykz1.png")
ASTER2 = simplegui.load_image("http://i.imgur.com/iGrYwJS.png")
ASTER3 = simplegui.load_image("http://i.imgur.com/MLDgSMk.png")
ASTER4 = simplegui.load_image("http://i.imgur.com/eOrtqH1.png")
ASTER5 = simplegui.load_image("http://i.imgur.com/gAjYgXG.png")
DEBRIS1 = simplegui.load_image("http://i.imgur.com/Vg2bNEG.png")
DEBRIS2 = simplegui.load_image("http://i.imgur.com/nkadPIz.png")
DEBRIS3 = simplegui.load_image("http://i.imgur.com/lK7av6M.png")
TURRET_BASE = simplegui.load_image("http://i.imgur.com/Z9gv582.png")
TURRET_ARM = simplegui.load_image("http://i.imgur.com/rT5fHWV.png")

BACK_WIDTH = BACK_IMAGE.get_width()
BACK_HEIGHT = BACK_IMAGE.get_height()
ARWING_WIDTH = ARWING_IMAGE.get_width()
ARWING_HEIGHT = ARWING_IMAGE.get_height()
AR_FRONT_WIDTH = ARWING_FRONT.get_width()
AR_FRONT_HEIGHT = ARWING_FRONT.get_height()
FOX_WIDTH = FOX_IMAGE.get_width()
FOX_HEIGHT = FOX_IMAGE.get_height()

FRAME_WIDTH = 500
FRAME_HEIGHT = 800
background_pos = [BACK_WIDTH / 2, 0]

while BACK_WIDTH == 0 or AR_FRONT_WIDTH == 0:
    pass  # wait for images to load


# Initializes all variables to prepare for a level and sets the enemies and bosses for each level.
# This function is only called when a level is started (in the mouse handler function).
def new_game():
    global player, level, boss, boss_fight, boss_defeat, boss_text_start, boss_text_end
    global b_spawn_list, e_spawn_list, rb_spawn_list
    global loading, load_time, loading_timer, level_time, points, boss_text_time, boss_name
    global enemy_list, building_list, round_build_list, bullet_list, ene_bullet_list, boss_list
    global GRANGA_HEIGHT, GRANGA_WIDTH, WOLFEN_HEIGHT, WOLFEN_WIDTH
    global ANDROSS_HEAD_HEIGHT, ANDROSS_HEAD_WIDTH, ANDROSS_HAND_HEIGHT, ANDROSS_HAND_WIDTH

    # reinitializing all variables and lists
    enemy_list = []
    building_list = []
    round_build_list = []
    bullet_list = []
    ene_bullet_list = []
    boss_list = []
    boss_bullet_list = []

    loading = True
    load_time = 3  # time in seconds
    boss_defeat = False
    boss_fight = False
    points = 0
    boss_text_start = []  # boss transmission to player before fight
    boss_text_end = []  # boss transmission to player after defeat
    boss_text_time = 5  # time in seconds
    boss_name = []  # boss' name and position to display
    
    """
    Spawn lists are 2D lists:
    [x][0] = time needed to pass since last spawn, [x][1] = ID value for preset building, [x][2] for position
    these set time and order to spawn buildings and enemies
    
    ID VALUES:
    1 = player
    2, 4 = Granga, Granga rematch
    3 = Mecha Turret
    5 = Star Wolf crew (5.1 = wolf, 5.2 = pigma)
    6 = Andross (6 = head, 6.1 = left hand, 6.2 = right hand)
    10 = fly
    20, 21, 22 = mosquito
    30 = hornet, three spawn at once
    40, 41, 42 = queen fly
    50 = mini andross
    100, 101, 102 = asteroid (round)
    110, 111 = small_asteroid (round)
    120, 121, 122 = debris (round)
    500, 501, 502 = satellite (square)
    510, 511, 512 = turret (square)
    1000, 1001, 1002 = ring (white ring, heal 15)
    1010, 1011, 1012 = yl_ring (yellow ring, heal 20, increase max shield by 10)
    1020, 1021, 1022 = gr_ring (green ring, heal 40, increase max shield by 10)
    1030, 1031, 1032 = re_ring (red ring, heal full, increase max shield by 5)
    """
    
    if level == 1:
        b_spawn_list = []  # square building spawn list
        e_spawn_list = [[55, 10], [45, 10], [38, 10], [34, 10], [32, 10], [30, 10], [25, 10],
                        [23, 10], [22, 10], [18, 10], [15, 10], [10, 10], [7, 10]]  # enemy spawn list
        rb_spawn_list = [[50, 100], [50, 110], [48, 100], [45, 110], [42, 111], [42, 1000], [40, 111],
                         [37, 110], [34, 102], [32, 110], [30, 100], [29, 1002], [27, 110], [26, 100],
                         [26, 101], [26, 110], [25, 110], [24, 100], [22, 102], [22, 110], [21, 111],
                         [21, 101], [20, 102], [17, 110], [15, 100], [12, 1001], [11, 101]]  # round building and ring spawn list
        level_time = 63 * 60  # (time in seconds for level + 3 seconds for loading screen) * 60 fps
        
        GRANGA_HEIGHT = GRANGA_IMAGE.get_height()
        GRANGA_WIDTH = GRANGA_IMAGE.get_width()

        # position, velocity, radius, colour, bullet, ID, score, shield, max_shield
        boss = Boss([70, 150], [4, 4], 50, "ff1111", None, 2, 150, 100, 100)
        boss_name = ["Granga", [216, 30]]

        boss_text_start = ["Star Fox! Andross sent me to stop you!", [35, 380]]  # text, position
        boss_text_end = ["Once Andross hears about this, you're", [40, 370],
                         "through, you hear me? Through!", [70, 400]]

    elif level == 2:
        b_spawn_list = [[44, 500], [30, 510], [20, 511], [17, 512], [15, 510], [12, 511], [10, 510], [8, 512]]
        e_spawn_list = [[55, 20], [50, 21], [45, 10], [38, 21], [35, 22], [32, 10], [30, 20], [27, 20],
                        [25, 22], [23, 21], [21, 10], [18, 22], [15, 10], [13, 21], [10, 22], [7, 20]]
        rb_spawn_list = [[60, 120], [57, 121], [55, 1002], [54, 100], [51, 122], [48, 101], [43, 102],
                        [42, 1010], [39, 122], [35, 1000], [32, 120], [30, 110], [28, 120], [26, 1011],
                        [24, 120], [21, 1002], [18, 100], [15, 111], [14, 1010], [10, 121], [8, 101], [6, 1000]]
        level_time = 63 * 60
        
        # Mecha turret image code is in the boss class.
        
        # position, velocity, radius = sqrt(2*30^2), colour, bullet, ID, score, shield, max_shield
        # sqrt(2*30^2) is the half the length of the diagonal of the square (square side length is 60)
        rad = math.sqrt(1800)
        boss = Boss([[235, 140], [235, 170], [265, 170], [265, 140]], [0, 0], rad, "ff1111", None, 3, 150, 80, 80)
        boss_name = ["Mecha Turret", [191, 30]]
        boss_text_start = ["INTRUDER DETECTED", [116, 380]]  # text, position
        boss_text_end = ["DAMAGE LEVEL EXCEEDED OperaTiNg", [16, 370],
                         "lEVel SYst-m shut-t^in", [125, 400],
                         "dw#o$%*8n)>-|---", [156, 430]]

    elif level == 3:
        b_spawn_list = [[85, 500], [79, 501], [75, 500], [71, 500], [67, 501], [63, 502], [60, 501], [55, 502],
                        [51, 500], [44, 500], [40, 502], [36, 501], [32, 500], [28, 502], [24, 500], [20, 501],
                        [16, 502], [11, 510]]
        e_spawn_list = [[90, 30], [87, 20], [84, 21], [80, 21], [76, 22], [73, 20], [70, 30], [68, 30], [65, 22],
                        [61, 20], [58, 20], [54, 22], [52, 30], [46, 21], [43, 20], [39, 30], [34, 21], [30, 20],
                        [27, 30], [22, 22], [19, 22], [14, 30], [10, 21], [8, 20]]
        rb_spawn_list = [[88, 101], [82, 1020], [80, 100], [78, 110], [66, 1011], [56, 111], [50, 1022], [48, 101],
                         [41, 121], [38, 122],[35, 1020], [26, 1002], [21, 120], [18, 102], [15, 1021], [12, 101],
                         [9, 122], [7, 1012]]
        level_time = 93 * 60
        
        GRANGA_HEIGHT = GRANGA_IMAGE.get_height()
        GRANGA_WIDTH = GRANGA_IMAGE.get_width()

        # position, velocity, radius, colour, bullet, ID, score, shield, max_shield
        boss = Boss([70, 150], [4, 4], 50, "ff1111", "target", 4, 150, 200, 200)
        boss_name = ["Granga", [216, 30]]

        boss_text_start = ["We meet again, Star Fox!", [110, 370],
                          "There's no escaping me this time!", [63, 400]]  # text, position
        boss_text_end = ["My emperor...I've failed you!", [95, 370]]

    elif level == 4:
        b_spawn_list = [[85, 500], [79, 501], [75, 500], [71, 500], [67, 501], [63, 502], [60, 501], [55, 502],
                        [51, 500], [44, 500], [40, 502], [36, 501], [33, 511], [32, 500], [29, 512], [28, 502],
                        [24, 500], [20, 501], [16, 502], [11, 510]]
        e_spawn_list = [[90, 30], [89, 22], [87, 20], [84, 21], [80, 21], [76, 30], [74, 40], [73, 20], [70, 30],
                        [68, 30], [65, 22], [62, 41], [61, 20], [58, 20], [54, 22], [52, 30], [49, 42], [46, 21],
                        [43, 20], [39, 30], [34, 21], [30, 20], [27, 30], [25, 40], [22, 22], [19, 22], [14, 30],
                        [10, 41], [8, 20]]
        rb_spawn_list = [[88, 101], [86, 121], [82, 1020], [80, 100], [78, 110], [66, 1011], [64, 102], [56, 111],
                         [50, 1022], [48, 101], [45, 110], [41, 121], [38, 122], [35, 1020], [26, 1010], [21, 120],
                         [18, 102], [15, 1022], [12, 101], [9, 122], [5, 1030]]
        level_time = 93 * 60
        
        WOLFEN_HEIGHT = WOLFEN_IMAGE.get_height()
        WOLFEN_WIDTH = WOLFEN_IMAGE.get_width()

        # position, velocity, radius, colour, bullet, ID, score, shield, max_shield
        boss1 = Boss([150, 150], [3, 3], 45, "ff1111", "slow", 5.1, 150, 150, 150)  # wolf
        boss2 = Boss([350, 150], [3, 3], 45, "ff1111", None, 5.2, 150, 150, 150)  # pigma
        boss_list = [boss1, boss2]
        boss_name = ["Team Star Wolf", [181, 30],
                    "Wolf O'Donnell", [25, 30], "Pigma Dengar", [345, 30]]

        boss_text_start = ['Star Fox! The "best pilot in the galaxy".', [36, 350],
                          "Looks like someone needs to put you in", [31, 380],
                          "your place. It's up to me and Pigma now!", [25, 410]]  # text, position
        boss_text_end = ["Grr... mark my words, Fox. This isn't", [50, 310],
                        "the last you'll see of us. You know", [65, 340],
                        "we're better than you!", [131, 370],
                        "...shut up, Pigma.", [154, 430]]

    elif level == 5:
        b_spawn_list = [[107, 501], [104, 502], [101, 500], [99, 501], [85, 502], [83, 501], [81, 502], [79, 500],
                        [75, 500], [71, 512], [67, 501], [63, 500], [60, 511], [57, 500], [53, 502], [51, 510],
                        [47, 501], [44, 502], [40, 512], [36, 500], [33, 510], [32, 502], [29, 511], [28, 500],
                        [16, 510], [13, 502], [11, 510], [6, 501]]
        e_spawn_list = [[120, 40], [116, 30], [114, 22], [112, 40], [111, 21], [110, 20], [108, 50], [105, 30],
                        [102, 41], [100, 21], [98, 21], [96, 22], [95, 30], [94, 42], [93, 40], [92, 30], [91, 22],
                        [90, 40], [89, 22], [87, 30], [84, 21], [80, 41], [76, 30], [74, 40], [73, 30], [70, 42],
                        [68, 50], [65, 30], [62, 42], [61, 22], [58, 21], [54, 40], [52, 30], [49, 22], [46, 41],
                        [43, 30], [39, 41], [37, 30], [34, 20], [30, 50], [27, 30], [25, 40], [22, 22], [20, 50],
                        [19, 22], [17, 30], [14, 30], [10, 41], [8, 40]]
        rb_spawn_list = [[119, 101], [118, 111], [117, 120], [115, 1000], [113, 122], [109, 1011], [108, 102],
                         [106, 110], [103, 1010], [97, 1030], [93, 1012], [88, 1022], [86, 120], [82, 121],
                         [80, 101], [78, 111], [77, 1011], [72, 1002], [66, 1010], [64, 100], [59, 1002], [56, 110],
                         [55, 1030], [50, 1011], [48, 102], [45, 111], [42, 1012], [41, 120], [38, 121], [35, 1022],
                         [31, 1001], [26, 1011], [24, 111], [23, 1032], [21, 122], [18, 101], [15, 1021], [12, 100],
                         [9, 121], [7, 1031], [5, 1030]]
        level_time = 123 * 60

        ANDROSS_HEAD_HEIGHT = ANDROSS_HEAD.get_height()
        ANDROSS_HEAD_WIDTH = ANDROSS_HEAD.get_width()
        ANDROSS_HAND_HEIGHT = ANDROSS_LH.get_height()
        ANDROSS_HAND_WIDTH = ANDROSS_LH.get_width()  # rh and lh have identical dimensions

        # position, velocity, radius, colour, bullet, ID, score, shield, max_shield
        boss = Boss([250, 150], [6, 6], 65, "ff1111", None, 6, 150, 225, 225)  # head, bullet type is random
        boss1 = Boss([200, 200], [6, 6], 25, "ff1111", "slow", 6.1, 150, 200, 200)  # LH
        boss2 = Boss([300, 200], [6, 6], 25, "ff1111", "depletion", 6.2, 150, 200, 200)  # RH
        boss_list = [boss, boss1, boss2]
        boss_name = ["ANDROSS", [202, 30], "Hand", [78, 30], "Hand", [376, 30]]

        boss_text_start = ["Star Fox.", [201, 370],
                          "This is your end.", [160, 430]]  # text, position
        boss_text_end = ["NO! How could this happen?! Not again...", [21, 370]]

    loading_timer = level_time

    shield = 50 * ((level / 10.0) + 0.9)  # starting shield increases each level
    # position, velocity, radius, colour, ID, shield, max_shield, impact
    player = Player([250, 625], [3, 3], 45, "#ff1111", 1, shield, shield, False)


# Functions to calculate distances/collisions/angles:
# round_dist is for collisions with round objects, angle is for orienting turret arm, and rect_dist
# is for collisions with square objects. The colliding object will always be round.
def round_dist(pos1, pos2):
    a = pos2[0] - pos1[0]
    b = pos2[1] - pos1[1]
    distance = math.sqrt(a ** 2 + b ** 2)
    return distance

    
def angle(pos1, pos2):
    # pos1 is player, pos2 is turret centre
    a = pos2[0] - pos1[0]
    b = pos2[1] - pos1[1]
    ang = math.atan2(b, a)
    return ang  # the last airbender? :P


def rect_dist(subject, pos):
    # subject is ROUND object, pos is a list of coordinates of the square object
    distance = 100  # 100 if object is outside of building, 0 if inside
    # Getting the coordinate of the side that changes perpendicular to the side (if that makes sense)
    side1 = pos[0][0]  # x coord of the left wall
    side2 = pos[1][1]  # y bottom wall
    side3 = pos[2][0]  # x right wall
    side4 = pos[3][1]  # y top wall

    if (subject.pos[0] + subject.radius) >= side1 and (subject.pos[0] - subject.radius) <= side3:
        if (subject.pos[1] - subject.radius) <= side2 and (subject.pos[1] + subject.radius) >= side4:
            distance = 0
    return distance


class Player:
    def __init__(self, position, velocity, radius, colour, ID, shield, max_shield, impact):
        self.pos = position
        self.vel = velocity
        self.ID = ID
        self.radius = radius
        self.colour = colour
        self.shield = shield
        self.max_shield = max_shield  # starts at 50
        self.shield_limit = 200  # limit on max shield
        self.slow_time = level_time  # time when slowing effect ends
        self.deplete_time = level_time  # time when shield depletion effect ends
        self.max_vel = [] 
        self.max_vel.extend(self.vel)  # velocity to return to when slowing effect ends
        self.impact = impact  # triggers flashing shield animation
        self.first_impact = False
        self.timer_end = 0
        self.dead = False

    def enemy_collided(self, enemy):
        if round_dist(enemy.pos, self.pos) < (enemy.radius + self.radius):
            self.shield -= 1
            self.impact = True
            self.first_impact = True

    def building_collided(self, building):  # square buildings
        if rect_dist(self, building.pos) < self.radius:
            self.shield -= 0.5
            self.impact = True
            self.first_impact = True

    def r_building_collided(self, building, pos):  # round buildings
        global round_build_list, points
        if round_dist(pos, self.pos) < (self.radius + building.radius):
            if building.ID < 1000:  # non-ring
                self.shield -= 0.5
                self.impact = True
                self.first_impact = True
            elif building.ID >= ID_dict["ring"]:  # ring
                points += building.score
                self.max_shield += building.max_increase
                self.shield += building.health
                if self.max_shield > self.shield_limit:
                    self.max_shield = self.shield_limit
                if self.shield > self.max_shield:
                    self.shield = self.max_shield
                round_build_list.remove(building)

    def bullet_collided(self, bullet):
        global ene_bullet_list
        if round_dist(bullet.pos, self.pos) < self.radius:
            self.shield -= 3
            ene_bullet_list.remove(bullet)
            self.impact = True
            self.first_impact = True
            
            if bullet.effect == "slow":  # only queen fly fires bullets with an effect
                self.slow_time = level_time - (3 * 60)  # 3 seconds
                self.vel = [1.5, 1.5]

    def boss_bullet_collided(self, bullet):
        global boss_bullet_list
        if round_dist(bullet.pos, self.pos) < self.radius:
            self.shield -= bullet.damage
            boss_bullet_list.remove(bullet)
            self.impact = True
            self.first_impact = True

            if bullet.effect == "slow":
                self.slow_time = level_time - (3 * 60)  # 3 seconds
                self.vel = [1.5, 1.5]
            elif bullet.effect == "depletion":
                self.deplete_time = level_time - (2 * 60)  # 2 seconds

    def draw(self, canvas, timer_start):
        canvas.draw_image(ARWING_IMAGE, (ARWING_WIDTH / 2, ARWING_HEIGHT / 2), (ARWING_WIDTH, ARWING_HEIGHT), 
                        (self.pos[0], self.pos[1] - 6), (self.radius * 2, self.radius * 2))

        # code to create flashing shield around player if hit
        if self.impact:
            if self.first_impact:
                self.timer_end = timer_start - (2 * 60)  # timer_start is measured in # of seconds * 60
                self.first_impact = False

            if timer_start >= self.timer_end:
                if level_time % 30 < 15:  # if hit
                    canvas.draw_circle(self.pos, self.radius + 1, 2, self.colour)
                if level_time % 20 < 10:  # if slow effect is applied
                    if self.vel != self.max_vel:
                        canvas.draw_circle(self.pos, self.radius - 1, 2, "#ffff00")
                else:  # if depletion effect is applied
                    if level_time > self.deplete_time:
                        canvas.draw_circle(self.pos, self.radius - 1, 2, "#ff00ff")
            else:
                self.impact = False
        
        else:  # slow effect lasts longer than animation timer, so this needs to also be outside self.impact if statement
            if level_time % 20 < 10:
                if self.vel != self.max_vel:
                    canvas.draw_circle(self.pos, self.radius - 1, 2, "#ffff00")

    def update(self):
        if boss_fight or boss_defeat:
            if moving_left and self.pos[0] > 0:
                self.pos[0] -= self.vel[0]
            if moving_right and self.pos[0] < FRAME_WIDTH:
                self.pos[0] += self.vel[0]
            if moving_up and self.pos[1] > (FRAME_HEIGHT / 2) + self.radius:
                self.pos[1] -= self.vel[1]
            if moving_down and self.pos[1] < FRAME_HEIGHT:
                self.pos[1] += self.vel[1]
        else:
            if moving_left and self.pos[0] > 0:
                self.pos[0] -= self.vel[0]
            if moving_right and self.pos[0] < FRAME_WIDTH:
                self.pos[0] += self.vel[0]
        
        if level_time <= self.slow_time and self.vel != self.max_vel:
            self.vel = [] 
            self.vel.extend(self.max_vel)
        if level_time > self.deplete_time:
            self.shield -= 0.1

    def draw_health(self, canvas):
        # Draws health bar for player
        health_bar_back_end = (float(self.max_shield) * 1.2) + 123
        canvas.draw_line([123, 45], [health_bar_back_end, 45], 12, "grey")
        health_bar_end = (float(self.max_shield) * (float(self.shield) / float(self.max_shield))) * 1.2 + 123
        if float(self.shield) / float(self.max_shield) > 0.25:
            canvas.draw_line([123, 45], [health_bar_end, 45], 12, "#22ff22")
        else:
            canvas.draw_line([123, 45], [health_bar_end, 45], 12, "#ff1111")  # health bar turns red at 25% health
        canvas.draw_text("Shield health", [30, 50], 15, "white", "sans-serif")

    def death(self):
        self.dead = True  # much code such wow


class Enemy:
    def __init__(self, position, velocity, radius, colour, ID, score, shield = 2):
        self.pos = position
        self.vel = velocity
        self.radius = radius
        self.colour = colour
        self.ID = ID
        self.score = score
        self.shield = shield  # defaults to 2 (2 hits to kill)
        self.impact = False
        self.first_impact = False
        self.timer_end = 0
        
        if self.ID == ID_dict["fly"]:
            self.image = FLY_IMG
        
        elif self.ID <= ID_dict["mosquito2"]:
            self.image = MOSQUITO_IMG
        
        elif self.ID == ID_dict["hornet"]:
            self.x_centre = self.pos[0]  # centre for hornet to swerve around
            self.vel[0] = random.randint(10, 40) / 10.0
            self.image = HORNET_IMG
        
        elif self.ID <= ID_dict["queen_fly2"]:
            self.image = QUEEN_IMG
        
        else:  # mini andross
            self.pos_x = random.randint(50, FRAME_WIDTH - 50)
            self.pos_y = random.randint(70, (FRAME_HEIGHT / 2) - 70)
            self.image = MINI_ANDROSS_IMG
            
        self.img_width = self.image.get_width()  # This is less efficient, but I don't feel like writing 150 lines of
        self.img_height = self.image.get_height()  # get_width code. This code occurs in the building classes as well.

    def draw(self, canvas):
        canvas.draw_image(self.image, (self.img_width / 2, self.img_height / 2), (self.img_width, self.img_height), 
                    (self.pos[0], self.pos[1]), (self.radius * 2, self.radius * 2))

        # code to create flashing shield around enemy if hit
        if self.impact:
            if self.first_impact:
                self.timer_end = level_time - (2 * 60)  # timer_start is measured in # of seconds * 60
                self.first_impact = False

            if level_time >= self.timer_end:
                if level_time % 30 < 15:
                    canvas.draw_circle(self.pos, self.radius + 1, 3, "#ff1111")
            else:
                self.impact = False

    def update(self):
        if self.ID == ID_dict["fly"]:
            self.pos[1] += self.vel[1]
        
        elif self.ID <= ID_dict["mosquito2"]:
            self.pos[1] += self.vel[1]
        
        elif self.ID <= ID_dict["hornet"]:
            self.pos[1] += self.vel[1]
            self.pos[0] += self.vel[0]
            if abs(self.pos[0] - self.x_centre) > 20:
                self.vel[0] *= -1
        
        elif self.ID <= ID_dict["queen_fly2"]:
            self.pos[1] += self.vel[1]
        
        elif self.ID == ID_dict["mini_andross"]:
            if level_time % (4 * 60) == 0:
                self.pos_x = random.randint(50, FRAME_WIDTH - 50)
                self.pos_y = random.randint(70, (FRAME_HEIGHT / 2) - 70)

            if abs(self.pos[0] - self.pos_x) > 3:
                if (self.pos[0] - self.pos_x) > 0:
                    self.pos[0] -= self.vel[0]
                else:
                    self.pos[0] += self.vel[0]
            if abs(self.pos[1] - self.pos_y) > 3:
                if (self.pos[1] - self.pos_y) > 0:
                    self.pos[1] -= self.vel[1]
                else:
                    self.pos[1] += self.vel[1]

    def shoot(self, random_num):
        if self.pos[1] <= player.pos[1]:  # only shoot if enemy is in front of the player
            if self.ID == ID_dict["fly"]:
                if random_num <= 75:
                    # starting pos, dest pos, vel
                    bullet_vel = bullet_target(self.pos, player.pos, 9)
                    # position, velocity, radius, colour, ID, effect (defaults to none)
                    ene_bullet = Bullet([self.pos[0], self.pos[1]], bullet_vel, 3, self.colour, -10)
                    ene_bullet_list.append(ene_bullet)
            elif self.ID <= ID_dict["mosquito2"]:
                # function only calls twice a second, no need to control how often it fires
                # starting pos, dest pos, vel
                bullet_vel = bullet_target(self.pos, player.pos, 11)
                ene_bullet = Bullet([self.pos[0], self.pos[1]], bullet_vel, 3, self.colour, -10)
                ene_bullet_list.append(ene_bullet)
            elif self.ID <= ID_dict["hornet"]:
                random_num = random.randint(0, 100)
                if random_num <= 60:
                    bullet_vel = bullet_target(self.pos, player.pos, 11)
                    ene_bullet = Bullet([self.pos[0], self.pos[1]], bullet_vel, 3, self.colour, -10)
                    ene_bullet_list.append(ene_bullet)
            elif self.ID <= ID_dict["queen_fly2"]:
                if random_num <= 60:
                    bullet_vel = bullet_target(self.pos, player.pos, 11)
                    ene_bullet = Bullet([self.pos[0], self.pos[1]], bullet_vel, 3, self.colour, -10, "slow")
                    ene_bullet_list.append(ene_bullet)
                elif random_num >= 70:
                    if random_num % 2 == 0:  # random chance to offset fly to either left or right of main ship
                        # position, velocity, radius, colour, ID, score, shield (defaults to 1)
                        fly = Enemy([self.pos[0] - self.radius, self.pos[1]], [5, 7], 20, "#ff1111", ID_dict["fly"], 5)
                    else:
                        fly = Enemy([self.pos[0] + self.radius, self.pos[1]], [5, 7], 20, "#ff1111", ID_dict["fly"], 5)
                    enemy_list.append(fly)
            elif self.ID == ID_dict["mini_andross"]:
                bullet_vel = bullet_target(self.pos, player.pos, 11)
                ene_bullet = Bullet([self.pos[0], self.pos[1]], bullet_vel, 3, self.colour, -10)
                ene_bullet_list.append(ene_bullet)


class Boss:
    def __init__(self, position, velocity, radius, colour, bullet, ID, score, shield, max_shield):
        global wolf_pos, boss_bullet_list
        self.pos = position  # set to 4 corners for mecha turret
        self.vel = velocity  # set to 4 corners for mecha turret
        self.radius = radius
        self.colour = colour
        self.bullet = bullet  # type of bullets that boss fires
        self.ID = ID
        self.score = score
        self.shield = shield
        self.max_shield = max_shield
        self.xdir = 1
        self.ydir = 1
        self.impact = False
        self.first_impact = False
        self.timer_end = 0
        boss_bullet_list = []

        # setting variables for bosses
        if self.ID == 3:  # mecha turret
            self.phase = 0
            self.centre = [(self.pos[0][0] + self.pos[2][0]) / 2, (self.pos[0][1] + self.pos[1][1]) / 2]
            self.base_img = TURRET_BASE
            self.arm_img = TURRET_ARM
            self.base_img_width = self.base_img.get_width()
            self.base_img_height = self.base_img.get_height()
            self.arm_img_width = self.arm_img.get_width()
            self.arm_img_height = self.arm_img.get_height()
            self.arm_rotation = 0
            self.side_length = (self.pos[2][0] + 30) - self.pos[0][0]  # base is square, add 30 for line width
            self.arm_width = 30
            self.arm_length = 150
            
        elif self.ID == 5.1:  # wolf
            self.pos_x = random.randint(50, FRAME_WIDTH - 50)
            self.pos_y = random.randint(70, (FRAME_HEIGHT / 2) - 70)
            wolf_pos = [self.pos_x, self.pos_y]
            
        elif self.ID == 5.2:  # pigma
            self.pos_x = random.randint(50, FRAME_WIDTH - 50)
            self.pos_y = random.randint(70, (FRAME_HEIGHT / 2) - 70)
            while round_dist([self.pos_x, self.pos_y], wolf_pos) <= self.radius * 2:
                self.pos_x = random.randint(50, FRAME_WIDTH - 50)
                self.pos_y = random.randint(70, (FRAME_HEIGHT / 2) - 70)
                
        elif self.ID == 6:  # Andross
            self.phase = 0
            # phase 0 = some hands remaining, 1 = only head remaining
            self.pos_x = self.pos[0]
            self.pos_y = self.pos[1]
            
        elif self.ID > 6:  # Andross hands
            self.pos_x = self.pos[0]
            self.pos_y = self.pos[1]

    def draw(self, canvas):
        if self.ID == 2 or self.ID == 4:  # granga
            canvas.draw_image(GRANGA_IMAGE, (GRANGA_WIDTH / 2, GRANGA_HEIGHT / 2), (GRANGA_WIDTH, GRANGA_HEIGHT),
                        self.pos, (self.radius * 2, self.radius * 2))
        
        elif self.ID == 3:  # mecha turret
            canvas.draw_image(self.base_img, (self.base_img_width / 2, self.base_img_height / 2),
                    (self.base_img_width, self.base_img_height), self.centre, (self.side_length, self.side_length))
            self.arm_rotation = (angle(player.pos, self.centre)) - (math.pi / 2)
            canvas.draw_image(self.arm_img, (self.arm_img_width / 2, self.arm_img_height / 2),
                    (self.arm_img_width, self.arm_img_height), self.centre, (self.arm_width, self.arm_length),
                    self.arm_rotation)
        
        elif 5 < self.ID < 6:  # star wolf
            canvas.draw_image(WOLFEN_IMAGE, (WOLFEN_WIDTH / 2, WOLFEN_HEIGHT / 2), (WOLFEN_WIDTH, WOLFEN_HEIGHT), 
                        (self.pos[0], self.pos[1] - 6), (95, 80))
        
        elif self.ID == 6:  # andross head
            canvas.draw_image(ANDROSS_HEAD, (ANDROSS_HEAD_WIDTH / 2, ANDROSS_HEAD_HEIGHT / 2), (ANDROSS_HEAD_WIDTH,
                            ANDROSS_HEAD_HEIGHT), self.pos, (ANDROSS_HEAD_WIDTH, ANDROSS_HEAD_HEIGHT))
        
        elif self.ID == 6.1:  # andross lh
            canvas.draw_image(ANDROSS_LH, (ANDROSS_HAND_WIDTH / 2, ANDROSS_HAND_HEIGHT / 2), (ANDROSS_HAND_WIDTH,
                            ANDROSS_HAND_HEIGHT), (self.pos[0], self.pos[1] - 4), (ANDROSS_HAND_WIDTH, ANDROSS_HAND_WIDTH))
        
        elif self.ID == 6.2:  # andross rh
            canvas.draw_image(ANDROSS_RH, (ANDROSS_HAND_WIDTH / 2, ANDROSS_HAND_HEIGHT / 2), (ANDROSS_HAND_WIDTH,
                            ANDROSS_HAND_HEIGHT), (self.pos[0], self.pos[1] - 4), (ANDROSS_HAND_WIDTH, ANDROSS_HAND_WIDTH))


        # code to create flashing shield around boss if hit
        if self.impact:
            if (self.ID != 3 and self.ID != 6) or (self.ID == 3 and self.phase == 0) or (self.ID == 6 and self.phase == 1):
                # all bosses except turret and andross head; mecha turret in 1st phase; andross head in last phase
                if self.first_impact:
                    self.timer_end = level_time - (2 * 60)  # timer_start is measured in # of seconds * 60
                    self.first_impact = False

                if level_time >= self.timer_end:
                    if level_time % 30 < 15:
                        if self.ID == 3:  # mecha turret only
                            canvas.draw_circle(self.centre, self.radius + 1, 3, "#ff1111")
                        else:
                            canvas.draw_circle(self.pos, self.radius + 1, 3, "#ff1111")
                            # for some reason self.colour doesn't work for these, so #ff1111 it is...
                else:
                    self.impact = False

    def update(self, randnum):
        global wolf_pos, lh_pos
        
        if self.ID == 2:  # Granga
            if 0 > self.pos[0] - self.radius or self.pos[0] + self.radius > FRAME_WIDTH:
                self.xdir *= -1
            if 0 > self.pos[1] - self.radius or self.pos[1] + self.radius > FRAME_HEIGHT / 2:
                self.ydir *= -1

            self.pos[0] += self.vel[0] * self.xdir
            self.pos[1] += self.vel[1] * self.ydir

            if randnum >= 97:
                # starting pos, dest pos, vel
                bullet_vel = bullet_target(self.pos, player.pos, 11)

                # position, velocity, radius, colour, ID, damage, effect = None
                boss_bullet = BossBullet([self.pos[0], self.pos[1] + 15], bullet_vel, 3, "#11ff11", -2, 6)
                boss_bullet_list.append(boss_bullet)

        elif self.ID == 3:  # mecha turret
            if level_time % 30 < 5:
                start_pos = [(self.pos[1][0] + self.pos[2][0]) / 2, self.pos[1][1]]
                # starting pos, dest pos, vel
                # change start pos to update with moving arm
                bullet_vel = bullet_target(start_pos, player.pos, 9)

                # position, velocity, radius, colour, ID, damage, effect = None
                boss_bullet = BossBullet([start_pos[0], start_pos[1]], bullet_vel, 1, "#11ff11", -3, 6)
                boss_bullet_list.append(boss_bullet)

        elif self.ID == 4:  # Granga rematch
            if 0 > self.pos[0] - self.radius or self.pos[0] + self.radius > FRAME_WIDTH:
                self.xdir *= -1
            if 0 > self.pos[1] - self.radius or self.pos[1] + self.radius > FRAME_HEIGHT / 2:
                self.ydir *= -1

            self.pos[0] += self.vel[0] * self.xdir
            self.pos[1] += self.vel[1] * self.ydir

            if randnum >= 96:
                # starting pos, dest pos, vel
                bullet_vel = bullet_target(self.pos, player.pos, 11)

                # position, velocity, radius, colour, ID, damage, effect = None
                boss_bullet = BossBullet([self.pos[0], self.pos[1] + 15], bullet_vel, 3, "#11ff11", -4, 6, self.bullet)
                boss_bullet_list.append(boss_bullet)
            elif randnum < 1:
                # position, velocity, radius, colour, ID, score, shield (defaults to 1)
                fly = Enemy([self.pos[0], self.pos[1] + 15], [5, 8], 20, "#ff1111", ID_dict["fly"], 5)
                enemy_list.append(fly)

        elif 5 < self.ID < 6:  # star wolf
            if level_time % (5 * 60) == 0:
                self.pos_x = random.randint(50, FRAME_WIDTH - 50)
                self.pos_y = random.randint(70, (FRAME_HEIGHT / 2) - 70)
                if self.ID == 5.1:  # wolf
                    wolf_pos = [self.pos_x, self.pos_y]
                elif self.ID == 5.2:  # pigma
                    while round_dist([self.pos_x, self.pos_y], wolf_pos) <= self.radius * 2:
                        self.pos_x = random.randint(50, FRAME_WIDTH - 50)
                        self.pos_y = random.randint(70, (FRAME_HEIGHT / 2) - 70)
            if math.fabs(self.pos[0] - self.pos_x) > 3:
                if (self.pos[0] - self.pos_x) > 0:
                    self.pos[0] -= self.vel[0]
                else:
                    self.pos[0] += self.vel[0]
            if math.fabs(self.pos[1] - self.pos_y) > 3:
                if (self.pos[1] - self.pos_y) > 0:
                    self.pos[1] -= self.vel[1]
                else:
                    self.pos[1] += self.vel[1]

            if abs(self.pos[0] - self.pos_x) <= 3 and abs(self.pos[1] - self.pos_y) <= 3:
                if level_time % 12 < 1:
                    # starting pos, dest pos, vel
                    bullet_vel = bullet_target(self.pos, player.pos, 11)

                    # position, velocity, radius, colour, ID, damage, effect = slow/None
                    if self.ID == 5.1:
                        if randnum >= 90:
                            boss_bullet = BossBullet([self.pos[0], self.pos[1] + 30], bullet_vel, 3, "#ffff00", -5, 3, self.bullet)
                        else:
                            boss_bullet = BossBullet([self.pos[0], self.pos[1] + 30], bullet_vel, 3, "#11ff11", -5, 4)
                    else:
                        boss_bullet = BossBullet([self.pos[0], self.pos[1] + 30], bullet_vel, 3, "#11ff11", -5, 4)
                    boss_bullet_list.append(boss_bullet)

        elif self.ID == 6:  # Andross
            if self.phase == 1:  # hands are dead
                if level_time % 30 == 0:
                    # starting pos, dest pos, vel
                    bullet_vel = bullet_target(self.pos, player.pos, 12)

                    # position, velocity, radius, colour, ID, damage, effect is randomised
                    randnumber = random.randrange(0, 100)
                    if randnumber < 33:
                        boss_bullet = BossBullet([self.pos[0], self.pos[1]], bullet_vel, 3, "#11ff11", -5, 3, None)
                    elif randnumber >= 66:
                        boss_bullet = BossBullet([self.pos[0], self.pos[1]], bullet_vel, 3, "#ffff00", -5, 3, "slow")
                    else:
                        boss_bullet = BossBullet([self.pos[0], self.pos[1]], bullet_vel, 3, "#ff00ff", -5, 3, "depletion")
                    boss_bullet_list.append(boss_bullet)
                
                if level_time % (3 * 60) == 0:
                    self.pos_x = random.randint(50, FRAME_WIDTH - 50)
                    self.pos_y = random.randint(70, (FRAME_HEIGHT / 2) - 70)
                if math.fabs(self.pos[0] - self.pos_x) > 3:
                    if (self.pos[0] - self.pos_x) > 0:
                        self.pos[0] -= self.vel[0]
                    else:
                        self.pos[0] += self.vel[0]
                if math.fabs(self.pos[1] - self.pos_y) > 3:
                    if (self.pos[1] - self.pos_y) > 0:
                        self.pos[1] -= self.vel[1]
                    else:
                        self.pos[1] += self.vel[1]
                        
                if level_time % 60 == 0:
                        temp = random.randrange(0, 100)
                        if temp <= 8:
                            ring = Ring([100, -10], [0, 9], 15, "white", ID_dict["ring"], 10, 15)
                            round_build_list.append(ring)
                        elif temp <= 12:
                            ring = Ring([250, -10], [0, 9], 15, "yellow", ID_dict["yl_ring1"], 10, 20, 10)
                            round_build_list.append(ring)
                        elif temp <= 15:
                            ring = Ring([375, -10], [0, 9], 15, "#22ff22", ID_dict["gr_ring2"], 10, 40, 10)
                            round_build_list.append(ring)
                        elif temp <= 17:
                            ring = Ring([250, -10], [0, 9], 15, "#ff1111", ID_dict["re_ring1"], 10, player.max_shield, 5)
                            round_build_list.append(ring)
                        
            if randnum <= 1:
                if level_time % 60 == 0:
                    # position, velocity, radius, colour, ID, score, shield (defaults to 2)
                    mini = Enemy([self.pos[0], self.pos[1]], [5, 5], 32, "#ff1111", ID_dict["mini_andross"], 30, 15)
                    enemy_list.append(mini)
                elif level_time % 60 < 15:
                    fly = Enemy([self.pos[0], self.pos[1]], [5, 8], 20, "#ff1111", ID_dict["fly"], 5)
                    enemy_list.append(fly)
                
        elif self.ID > 6:  # Andross hands
            if self.ID == 6.1:  # left
                if level_time % (4 * 60) == 0:
                    self.pos_x = random.randint(50, (FRAME_WIDTH / 2) + 50)
                    self.pos_y = random.randint(70, (FRAME_HEIGHT / 2) - 70)
                lh_pos = [self.pos_x, self.pos_y]
                if level_time % 60 == 30:
                    # starting pos, dest pos, vel
                    bullet_vel = bullet_target(self.pos, player.pos, 11)

                    # position, velocity, radius, colour, ID, damage, effect = slow
                    randnumber = random.randrange(0, 100)
                    if randnumber < 66:
                        boss_bullet = BossBullet([self.pos[0], self.pos[1]], bullet_vel, 3, "#11ff11", -5, 3, None)  # regular
                    else:
                        boss_bullet = BossBullet([self.pos[0], self.pos[1]], bullet_vel, 3, "#ffff00", -5, 3, self.bullet)  # slow
                    boss_bullet_list.append(boss_bullet)
                    
            else:  # right
                if level_time % (4 * 60) == 0:
                    self.pos_x = random.randint((FRAME_WIDTH / 2) - 50, FRAME_WIDTH - 50)
                    self.pos_y = random.randint(70, (FRAME_HEIGHT / 2) - 70)
                while round_dist([self.pos_x, self.pos_y], lh_pos) <= self.radius * 2:
                    self.pos_x = random.randint((FRAME_WIDTH / 2) - 50, FRAME_WIDTH - 50)
                    self.pos_y = random.randint(70, (FRAME_HEIGHT / 2) - 70)
                if level_time % 60 == 0:
                    bullet_vel = bullet_target(self.pos, player.pos, 11)

                    # position, velocity, radius, colour, ID, damage, effect = depletion
                    randnumber = random.randrange(0, 100)
                    if randnumber < 33:
                        boss_bullet = BossBullet([self.pos[0], self.pos[1]], bullet_vel, 3, "#ff00ff", -5, 3, self.bullet)  # depletion
                    else:
                        boss_bullet = BossBullet([self.pos[0], self.pos[1]], bullet_vel, 3, "#11ff11", -5, 3, None)  # regular
                    boss_bullet_list.append(boss_bullet)
                    
            if math.fabs(self.pos[0] - self.pos_x) > 3:
                if (self.pos[0] - self.pos_x) > 0:
                    self.pos[0] -= self.vel[0]
                else:
                    self.pos[0] += self.vel[0]
                    
            if math.fabs(self.pos[1] - self.pos_y) > 3:
                if (self.pos[1] - self.pos_y) > 0:
                    self.pos[1] -= self.vel[1]
                else:
                    self.pos[1] += self.vel[1]

    def bullet_collided(self, bullet):
        global bullet_list
        if self.ID == 2 or self.ID == 4:  # Granga
            if round_dist(bullet.pos, self.pos) < self.radius:
                self.shield -= 3
                bullet_list.remove(bullet)
                self.impact = True
                self.first_impact = True
        elif self.ID == 3:  # mecha turret (round shield)
            if round_dist(bullet.pos, self.centre) < self.radius:
                self.shield -= 2
                bullet_list.remove(bullet)
                self.impact = True
                self.first_impact = True
                if self.shield <= 0 and self.phase == 0:
                    self.phase = 1
                    self.shield = self.max_shield
        elif 5 < self.ID < 6:  # star wolf
            if round_dist(bullet.pos, self.pos) < self.radius:
                self.shield -= 2
                bullet_list.remove(bullet)
                self.impact = True
                self.first_impact = True
        elif self.ID == 6 and self.phase == 1:  # andross head
            if round_dist(bullet.pos, self.pos) < self.radius:
                self.shield -= 2
                bullet_list.remove(bullet)
                self.impact = True
                self.first_impact = True
        elif self.ID > 6:  # andross hands
            if round_dist(bullet.pos, self.pos) < self.radius:
                self.shield -= 2
                bullet_list.remove(bullet)
                self.impact = True
                self.first_impact = True

    def draw_health(self, canvas):
        # Draws health bar for boss
        if self.ID < 5:
            canvas.draw_line([0, 5], [500, 5], 10, "grey")
            health_bar_end = 500 * (float(self.shield) / float(self.max_shield))
            canvas.draw_line([0, 5], [health_bar_end, 5], 10, "#ff1111")
            canvas.draw_text(boss_name[0], boss_name[1], 20, "white", "sans-serif")

        elif self.ID < 6:  # Star Wolf team
            canvas.draw_text(boss_name[0], boss_name[1], 20, "white", "sans-serif")
            if self.ID == 5.1:
                canvas.draw_line([0, 5], [250, 5], 10, "grey")
                health_bar_end = 250 * (float(self.shield) / float(self.max_shield))
                canvas.draw_line([0, 5], [health_bar_end, 5], 10, "#ff1111")
                canvas.draw_text(boss_name[2], boss_name[3], 20, "#00ccff", "sans-serif")

            elif self.ID == 5.2:
                canvas.draw_line([250, 5], [500, 5], 10, "grey")
                health_bar_end = (250 * (float(self.shield) / float(self.max_shield))) + 250
                canvas.draw_line([250, 5], [health_bar_end, 5], 10, "#ff1111")
                canvas.draw_text(boss_name[4], boss_name[5], 20, "#00ccff", "sans-serif")

        elif self.ID == 6:  # andross head
            canvas.draw_text(boss_name[0], boss_name[1], 20, "white", "sans-serif")
            if self.phase == 1:
                canvas.draw_line([0, 5], [500, 5], 10, "grey")
                health_bar_end = 500 * (float(self.shield) / float(self.max_shield))
                canvas.draw_line([0, 5], [health_bar_end, 5], 10, "#ff1111")
                canvas.draw_text(boss_name[0], boss_name[1], 20, "white", "sans-serif")

        elif self.ID > 6:  # andross hands
            if self.ID == 6.1:
                canvas.draw_line([0, 5], [250, 5], 10, "grey")
                health_bar_end = 250 * (float(self.shield) / float(self.max_shield))
                canvas.draw_line([0, 5], [health_bar_end, 5], 10, "#ff1111")
                canvas.draw_text(boss_name[2], boss_name[3], 20, "#00ccff", "sans-serif")

            elif self.ID == 6.2:
                canvas.draw_line([250, 5], [500, 5], 10, "grey")
                health_bar_end = (250 * (float(self.shield) / float(self.max_shield))) + 250
                canvas.draw_line([250, 5], [health_bar_end, 5], 10, "#ff1111")
                canvas.draw_text(boss_name[4], boss_name[5], 20, "#00ccff", "sans-serif")

    def death(self):
        global points, boss_defeat, boss_fight, level_completed, end_time

        if self.ID <= 4:
            end_time = level_time
            points += self.score
            boss_defeat = True
            boss_fight = False
            if level > level_completed:
                level_completed = level
        elif 5 < self.ID < 6:
            if len(boss_list) == 2:  # 1 boss left
                points += self.score
            elif len(boss_list) == 1:  # no bosses left
                end_time = level_time
                points += self.score
                boss_defeat = True
                boss_fight = False
                if level > level_completed:
                    level_completed = level
        elif self.ID == 6:
            end_time = level_time
            points += self.score
            boss_defeat = True
            boss_fight = False
        elif self.ID > 6:
            if len(boss_list) == 3:  # 1 hand left
                points += self.score
            elif len(boss_list) == 2:  # no hands left
                points += self.score
                boss_list[0].phase = 1


class SquareBuilding:
    def __init__(self, position, velocity, colour, ID, score, shield = 1):
        self.pos = position
        self.vel = velocity
        self.colour = colour
        self.ID = ID
        self.score = score
        self.shield = shield  # defaults to 1 (1 hit to kill)
        self.impact = False
        self.first_impact = False
        self.timer_end = 0
        self.centre = [(self.pos[0][0] + self.pos[2][0]) / 2.0, (self.pos[0][1] + self.pos[2][1]) / 2.0]
        self.side_length = self.pos[2][0] - self.pos[0][0]  # base is square
        
        if ID_dict["satellite"] <= self.ID <= ID_dict["satellite2"]:
            direction = random.choice([-1, 1])
            y_vel = random.randint(50, 70) / 10.0
            x_vel = random.randint(0, 100) / 100.0 * direction
            self.vel = [x_vel, y_vel]
            
            if self.ID == ID_dict["satellite"]:
                self.image = SAT1
            elif self.ID == ID_dict["satellite1"]:
                self.image = SAT2
            elif self.ID == ID_dict["satellite2"]:
                self.image = SAT3
                
            self.img_width = self.image.get_width()
            self.img_height = self.image.get_height()
            self.rotate_spd = random.randrange(-40, 40) / 1000.0  # how quickly the image rotates
            self.rotation = 0
            
        elif self.ID >= ID_dict["turret"]:
            self.base_img = TURRET_BASE
            self.arm_img = TURRET_ARM
            
            self.base_img_width = self.base_img.get_width()
            self.base_img_height = self.base_img.get_height()
            self.arm_img_width = self.arm_img.get_width()
            self.arm_img_height = self.arm_img.get_height()
            self.arm_rotation = 0
            self.arm_width = 20
            self.arm_length = 100

    def draw(self, canvas):
        self.centre = [(self.pos[0][0] + self.pos[2][0]) / 2.0, (self.pos[0][1] + self.pos[2][1]) / 2.0]
        if self.ID >= ID_dict["turret"]:
            # code to rotate turret arm and create flashing shield
            self.arm_rotation = (angle(player.pos, self.centre)) - (math.pi / 2)
            canvas.draw_image(self.base_img, (self.base_img_width / 2, self.base_img_height / 2),
                    (self.base_img_width, self.base_img_height), self.centre, (self.side_length, self.side_length))
            canvas.draw_image(self.arm_img, (self.arm_img_width / 2, self.arm_img_height / 2),
                    (self.arm_img_width, self.arm_img_height), self.centre, (self.arm_width, self.arm_length),
                    self.arm_rotation)
            
            if self.impact:
                if self.first_impact:
                    self.timer_end = level_time - (2 * 60)  # timer_start is measured in # of seconds * 60
                    self.first_impact = False

                if level_time >= self.timer_end:
                    if level_time % 30 < 15:
                        canvas.draw_polygon(self.pos, 3, "#ff1111")
                else:
                    self.impact = False
        else:  # satellite
            canvas.draw_image(self.image, (self.img_width / 2, self.img_height / 2), (self.img_width, self.img_height),
                    self.centre, (self.side_length, self.side_length), (self.rotation % (2 * math.pi)))
            self.rotation += self.rotate_spd

    def update(self):
        for i in range(2):
            for j in range(4):
                self.pos[j][i] += self.vel[i]
        if ID_dict["turret"] <= self.ID <= ID_dict["turret2"]:
            if level_time % 60 == 0:
                start_pos = [(self.pos[1][0] + self.pos[2][0]) / 2, self.pos[1][1]]
                # starting pos, dest pos, vel
                bullet_vel = bullet_target(start_pos, player.pos, 9)

                # position, velocity, radius, colour, ID
                ene_bullet = Bullet([start_pos[0], start_pos[1]], bullet_vel, 4, "#ff1111", -10)
                ene_bullet_list.append(ene_bullet)


class RoundBuilding:
    def __init__(self, position, velocity, radius, colour, ID, score):
        self.pos = position
        self.vel = velocity
        self.radius = radius
        self.colour = colour
        self.ID = ID
        self.score = score
        
        if ID_dict["debris"] <= self.ID <= ID_dict["debris2"]:
            direction = random.choice([-1, 1])
            y_vel = random.randint(50, 70) / 10.0
            x_vel = random.randint(0, 100) / 100.0 * direction
            self.vel = [x_vel, y_vel]  # chooses a random direction to move in
        
        if self.ID < ID_dict["ring"]:  # non-ring buildings
            if self.ID == ID_dict["asteroid"]:
                self.image = ASTER1
            elif self.ID == ID_dict["asteroid1"]:
                self.image = ASTER2
            elif self.ID == ID_dict["asteroid2"]:
                self.image = ASTER3
            elif self.ID == ID_dict["small_asteroid"]:
                self.image = ASTER4
            elif self.ID == ID_dict["small_asteroid1"]:
                self.image = ASTER5
            elif self.ID == ID_dict["debris"]:
                self.image = DEBRIS1
            elif self.ID == ID_dict["debris1"]:
                self.image = DEBRIS2
            else:
                self.image = DEBRIS3
                
            self.img_width = self.image.get_width()
            self.img_height = self.image.get_height()
            self.rotate_spd = random.randrange(-70, 70) / 1000.0  # how quickly the image rotates
            self.rotation = 0
            
    def draw(self, canvas):
        if self.ID >= ID_dict["ring"]:  # rings only
            canvas.draw_circle(self.pos, self.radius, 1, "black", self.colour)
        else:
            canvas.draw_image(self.image, (self.img_width / 2, self.img_height / 2), (self.img_width, self.img_height),
                    (self.pos[0], self.pos[1]), (self.radius * 2, self.radius * 2), (self.rotation % (2 * math.pi)))
            self.rotation += self.rotate_spd
            
    def update(self):
        self.pos[0] += self.vel[0]
        self.pos[1] += self.vel[1]


class Bullet:
    def __init__(self, position, velocity, radius, colour, ID, effect = None):
        self.pos = position
        self.vel = velocity
        self.radius = radius
        self.colour = colour
        self.ID = ID
        self.effect = effect  # each bullet's effect (only choices are slow or none for regular bullets)

    def draw(self, canvas):
        canvas.draw_circle(self.pos, self.radius, 1, self.colour, self.colour)

    def update(self):
        self.pos[0] += self.vel[0]
        self.pos[1] += self.vel[1]


class BossBullet(Bullet):
    def __init__(self, position, velocity, radius, colour, ID, damage, effect = None):
        Bullet.__init__(self, position, velocity, radius, colour, ID)
        self.damage = damage  # how much damage each bullet does to the player
        self.effect = effect  # each bullet's effect (slow, depletion, target, or none)
        if self.ID == -4:  # Granga rematch bullet
            self.start_vel = 9

    def draw(self, canvas):
        canvas.draw_circle(self.pos, self.radius, 2, self.colour, self.colour)

    def update(self):
        if self.effect == "target":
            if round_dist(self.pos, player.pos) <= 150 and level_time % 15 == 0:
                # start pos, dest_pos, vel (default 9 for granga rematch)
                vel = bullet_target(self.pos, player.pos, self.start_vel)
                self.vel[0]  = self.vel[0] * 0.75 + vel[0] * 0.25
                self.vel[1]  = self.vel[1] * 0.75 + vel[1] * 0.25
        Bullet.update(self)


class Ring:
    def __init__(self, position, velocity, radius, colour, ID, score, health, max_increase = 0):
        self.pos = position
        self.vel = velocity
        self.radius = radius
        self.colour = colour
        self.ID = ID
        self.score = score
        self.health = health  # amount of shield that ring restores
        self.max_increase = max_increase  # amount to increase shield maximum; defaults to 0

    def draw(self, canvas):
        canvas.draw_circle(self.pos, self.radius, 3, self.colour)

    def update(self):
        self.pos[0] += self.vel[0]
        self.pos[1] += self.vel[1]


class Button:
    def __init__(self, posx, posy, text, text_coords, outline_colour, colour, dest_level, on):
        self.posx = posx  # list of x coords
        self.posy = posy  # list of y coords
        self.text = text
        self.text_coords = text_coords
        self.out_colour = outline_colour
        self.colour = colour
        self.level = dest_level  # level to go to after click
        self.on = on  # if button is active

    def draw(self, canvas):
        if self.on:
            canvas.draw_polygon(
                [[self.posx[0], self.posy[0]], [self.posx[0], self.posy[1]], [self.posx[1], self.posy[1]],
                 [self.posx[1], self.posy[0]]], 1, self.out_colour, self.colour)
            if self.level == -4:  # button to background story; font is smaller size
                canvas.draw_text(self.text, self.text_coords, 18, "white", "sans-serif")
            else:
                canvas.draw_text(self.text, self.text_coords, 25, "white", "sans-serif")
            
        else:  # draw a black box for inactive buttons
            canvas.draw_polygon(
                [[self.posx[0], self.posy[0]], [self.posx[0], self.posy[1]], [self.posx[1], self.posy[1]],
                 [self.posx[1], self.posy[0]]], 1, "black", "black")

    def click(self, position):
        if self.posx[0] <= position[0] <= self.posx[1]:
            if self.posy[0] <= position[1] <= self.posy[1]:
                return True  # returns True if clicked
            else:
                return False
        else:
            return False

   
# All buttons are initialized here.   
def start():
    global level_select, help_button, options, back, level_back
    global level1, level2, level3, level4, level5
    global wasd_button, arrow_button, intro
    # Button class format:
    # pos_x, pos_y, text, text_coords, outline_colour, colour, dest_level, on
    level_select = Button([150, 350], [370, 420], "Level Select", [182, 404], "white", "blue", -1, True)
    help_button = Button([150, 350], [470, 520], "Help", [225, 504], "grey", "grey", -2, True)
    options = Button([150, 350], [570, 620], "Options / About", [164, 604], "grey", "grey", -3, True)
    back = Button([300, 450], [675, 725], "Back", [347, 708], "grey", "grey", 0, True)
    level_back = Button([300, 450], [675, 725], "Back", [347, 708], "grey", "grey", -1, True)
    
    intro = Button([160, 340], [155, 180], "Background Story", [179, 173], "white", "grey", -4, True)
    level1 = Button([125, 200], [225, 300], "1", [156, 270], "white", "blue", 1, True)
    level2 = Button([300, 375], [225, 300], "2", [331, 270], "white", "blue", 2, False)
    level3 = Button([125, 200], [375, 450], "3", [156, 421], "white", "blue", 3, False)
    level4 = Button([300, 375], [375, 450], "4", [331, 421], "white", "blue", 4, False)
    level5 = Button([213, 288], [525, 600], "5", [244, 571], "white", "#ff1111", 5, False)

    wasd_button = Button([225, 325], [175, 225], "", [256, 210], "white", "blue", -3, WASD)
    arrow_button = Button([375, 475], [175, 225], "", [456, 210], "white", "blue", -3, not WASD)


def draw(canvas):
    global background_pos, points
    global boss_fight, boss_defeat, boss
    global loading, load_time, level_time

    canvas.draw_image(BACK_IMAGE, (BACK_WIDTH / 2, BACK_HEIGHT / 2), (BACK_WIDTH, BACK_HEIGHT), 
                background_pos, (BACK_WIDTH, BACK_HEIGHT))
    # Code to draw the menu
    if level == 0:
        canvas.draw_text("STAR FOX 2D", [90, 100], 50, "white", "sans-serif")
        canvas.draw_image(ARWING_FRONT, (AR_FRONT_WIDTH / 2.0, AR_FRONT_HEIGHT / 2.0), (AR_FRONT_WIDTH, AR_FRONT_HEIGHT),
                          [255, 242], (275, 275))
        level_select.draw(canvas)
        help_button.draw(canvas)
        options.draw(canvas)

    elif level == -1:  # Level Select
        canvas.draw_text("Level Select", [172, 100], 30, "white", "sans-serif")
        intro.draw(canvas)
        back.draw(canvas)
        level1.draw(canvas)
        level2.draw(canvas)
        level3.draw(canvas)
        level4.draw(canvas)
        level5.draw(canvas)

    elif level == -2:  # Help
        canvas.draw_text("Help", [219, 100], 30, "white", "sans-serif")

        if WASD:
            canvas.draw_text("Use WASD to control the Arwing!", [108, 150], 20, "white", "sans-serif")
        else:
            canvas.draw_text("Use the arrow keys to control the Arwing!", [70, 150], 20, "white", "sans-serif")
        canvas.draw_text("Click anywhere on screen to shoot lasers.", [68, 175], 20, "white", "sans-serif")
        canvas.draw_text("Destroy enemies and obstacles to get points.", [57, 200], 20, "white", "sans-serif")

        canvas.draw_text("Running into enemies, obstacles, or their bullets", [37, 250], 20, "white", "sans-serif")
        canvas.draw_text("will deplete your shield health. If your shield drops", [30, 275], 20, "white", "sans-serif")
        canvas.draw_text("to 0, it's game over!", [167, 300], 20, "white", "sans-serif")

        canvas.draw_text("However, you want to run into rings. They will give", [28, 350], 20, "white", "sans-serif")
        canvas.draw_text("you points and restore your shield, and some can", [31, 375], 20, "white", "sans-serif")
        canvas.draw_text("have other effects. Find out what they are!", [62, 400], 20, "white", "sans-serif")

        canvas.draw_text("There is a boss battle at the end of each level.", [47, 450], 20, "white", "sans-serif")
        canvas.draw_text("While in this battle, the Arwing enters All-Range Mode,", [10, 475], 20, "white", "sans-serif")
        canvas.draw_text("meaning that you can move freely around the screen.", [15, 500], 20, "white", "sans-serif")
        canvas.draw_text("However, while not in All-Range Mode, you can only", [20, 525], 20, "white", "sans-serif")
        canvas.draw_text("move left and right.", [166, 550], 20, "white", "sans-serif")
        
        canvas.draw_text("Have fun discovering different enemies, bosses, and", [19, 600], 20, "white", "sans-serif")
        canvas.draw_text("effects on your way to Andross, but remember to do", [21, 625], 20, "white", "sans-serif")
        canvas.draw_text("other things too. Like eat and go outside.", [71, 650], 20, "white", "sans-serif")
        
        canvas.draw_text("Oh, and do read the background story.", [10, 765], 18, "white", "sans-serif")
        canvas.draw_text("It might help clear some things up.", [10, 787], 18, "white", "sans-serif")

        back.draw(canvas)

    elif level == -3:  # Options / About
        canvas.draw_text("Options / About", [147, 100], 30, "white", "sans-serif")

        canvas.draw_text("Control scheme:", [50, 209], 20, "white", "sans-serif")
        wasd_button.draw(canvas)
        arrow_button.draw(canvas)
        canvas.draw_text("WASD", [239, 209], 25, "white", "sans-serif")
        canvas.draw_text("Arrows", [387, 209], 25, "white", "sans-serif")
        
        canvas.draw_text("Written by Jonathan Wang, Mar. 9 - May 24, 2017", [31, 300], 20, "white", "sans-serif")
        canvas.draw_text("This game is supposed to make you rage quit. :)", [38, 350], 20, "white", "sans-serif")
        canvas.draw_text("Thanks to Ms. Stusiak, Richard Gan, and Andrew Luo", [12, 375], 20, "white", "sans-serif")
        canvas.draw_text("for help with (many) various issues!", [94, 400], 20, "white", "sans-serif")
        
        canvas.draw_text("Also, the name of this game and the names in it are", [22, 450], 20, "white", "sans-serif")
        canvas.draw_text("totally not property of Nintendo. Don't worry about it.", [21, 475], 20, "white", "sans-serif")
        canvas.draw_text("All sprites and images that appear in this game have", [18, 500], 20, "white", "sans-serif")
        canvas.draw_text("been edited by me, but are not my original art. Please", [13, 525], 20, "white", "sans-serif")
        canvas.draw_text("don't sue me. :P", [179, 550], 20, "white", "sans-serif")
        
        canvas.draw_text("...no, there is no auto-fire option. Enjoy your carpal", [27, 600], 20, "white", "sans-serif")
        canvas.draw_text("tunnel syndrome! (In all seriousness though, I tested", [19, 625], 20, "white", "sans-serif")
        canvas.draw_text("that auto-fire would make the game too easy.)", [48, 650], 20, "white", "sans-serif")
        
        # canvas.draw_text("Dedicated to my dad. I love you <3", [50, 700], 20, "white", "sans-serif")

        back.draw(canvas)
        
    elif level == -4:  # Background Story (intro)
        canvas.draw_text("Background", [170, 100], 30, "white", "sans-serif")
        
        canvas.draw_text("The evil lord Andross is threatening the planet Corneria", [6, 200], 20, "white", "sans-serif")
        canvas.draw_text("with total destruction. As the elite pilot, you, Fox", [40, 225], 20, "white", "sans-serif")
        canvas.draw_text("McCloud, have been hired to stop him. Take the fight to", [6, 250], 20, "white", "sans-serif")
        canvas.draw_text("his lair and destroy Andross before it's too late.", [44, 275], 20, "white", "sans-serif")
        
        canvas.draw_text("You will meet servants of and mercenaries hired by", [25, 325], 20, "white", "sans-serif")
        canvas.draw_text("Andross along the way. Good luck!", [97, 350], 20, "white", "sans-serif")
        
        canvas.draw_text("Don't worry, Andross is not your father; he was actually killed by", [45, 400], 15, "white", "sans-serif")
        canvas.draw_text("Andross. This is true from every point of view. :)", [97, 420], 15, "white", "sans-serif")
        
        canvas.draw_text("I'll do my best.", [289, 495], 20, "white", "sans-serif")
        canvas.draw_text("Andross won't have his way", [230, 520], 20, "white", "sans-serif")
        canvas.draw_text("with me!", [316, 545], 20, "white", "sans-serif")
        canvas.draw_polyline(([352, 555], [343, 562], [332, 569], [320, 575], [309, 580], [297, 584], [285, 587], [275, 589], 
                    [262, 591], [250, 592], [240, 592]), 3, "white")  # "speech line" (excuse my art skills)
        canvas.draw_image(FOX_IMAGE, (FOX_WIDTH / 2, FOX_HEIGHT / 2), (FOX_WIDTH, FOX_HEIGHT),
                    [146, 630], (FOX_WIDTH / 1.6, FOX_HEIGHT / 1.6))
        
        level_back.draw(canvas)

    elif level > 0:  # in game
        level_time -= 1  # timer for level, boss spawns when timer reaches 0
        random_number = random.randrange(0, 100)  # used for AI movements, firing, etc.
        background_pos[1] = (background_pos[1] + 5) % (BACK_HEIGHT / 2)

        if player.dead:
            canvas.draw_text("Game Over...", [177, 110], 25, "white", "sans-serif")
            canvas.draw_text("Points: " + str(points), [190, 150], 25, "white", "sans-serif")
            level_back.draw(canvas)

        elif loading:
            if not boss_fight:  # screen shown before level starts
                canvas.draw_text("Ready?", [211, 384], 25, "white", "sans-serif")
                player.draw(canvas, 0)
                player.update()
                
                if level_time <= loading_timer - load_time * 60:
                    loading = False
			
            else:  # screen shown before boss fight
                if level_time >= loading_timer - load_time * 60:
                    canvas.draw_text("WARNING: BOSS APPROACHING", [53, 384], 25, "white", "sans-serif")
                elif level_time >= loading_timer - (load_time + boss_text_time) * 60:
                    canvas.draw_text("TRANSMISSION", [156, 95], 25, "white", "sans-serif")
                    for i in range(len(boss_text_start)):
                        if i % 2 == 0:
                            if level < 5:
                                canvas.draw_text(boss_text_start[i], boss_text_start[i + 1], 25, "white", "sans-serif")
                            elif level == 5:
                                canvas.draw_text(boss_text_start[i], boss_text_start[i + 1], 25, "#ff1111", "sans-serif")

                player.draw(canvas, 0)
                player.update()
                player.draw_health(canvas)
                point_text = "Points: " + str(points)
                canvas.draw_text(point_text, [380, 50], 15, "white", "sans-serif")
                canvas.draw_text("ALL-RANGE MODE ACTIVE", [122, 790], 20, "white", "sans-serif")

                for bullet in bullet_list:
                    bullet.draw(canvas)
                    bullet.update()
                    
                for bullet in ene_bullet_list:
                    bullet.draw(canvas)
                    bullet.update()
                    player.bullet_collided(bullet)

                if level_time <= loading_timer - (load_time + boss_text_time) * 60:
                    loading = False  # when timer runs out, boss fight starts

        elif not boss_defeat:
            point_text = "Points: " + str(points)
            canvas.draw_text(point_text, [380, 50], 15, "white", "sans-serif")

            player.draw(canvas, level_time)
            player.update()
            player.draw_health(canvas)

            if boss_fight:
                canvas.draw_text("ALL-RANGE MODE ACTIVE", [122, 790], 20, "white", "sans-serif")
                
                for bullet in boss_bullet_list:
                    bullet.draw(canvas)
                    bullet.update()
                    player.boss_bullet_collided(bullet)

                # Displays "barrier" (moving green line that player can't cross)
                if player.pos[1] == (FRAME_HEIGHT / 2) + player.radius:
                    if level_time % 20 < 10:
                        canvas.draw_line([0, FRAME_HEIGHT / 2 - 2], [FRAME_WIDTH, FRAME_HEIGHT / 2 - 2], 5, "green")
                    else:
                        canvas.draw_line([0, FRAME_HEIGHT / 2 + 3], [FRAME_WIDTH, FRAME_HEIGHT / 2 + 3], 5, "green")

                if level < 4:  # both granga fights and mecha turret fight
                    if boss.shield <= 0:
                        boss.death()
                    else:
                        boss.draw(canvas)
                        boss.update(random_number)
                        boss.draw_health(canvas)
                else:  # team star wolf and andross fights work and end differently from earlier fights
                    temp_boss_list = []
                    temp_boss_list.extend(boss_list)
                    for boss in temp_boss_list:
                        if boss.shield <= 0:
                            boss.death()
                            boss_list.remove(boss)
                        else:
                            boss.draw(canvas)
                            boss.update(random_number)
                            boss.draw_health(canvas)
            
            if level_time % 60 == 0:  # calls once per second
                if not boss_fight:
                    spawn()  # any bosses that spawn in enemies will have their own spawn function
                despawn()

            if player.shield < 0:
                player.death()

            for enemy in enemy_list:
                enemy.draw(canvas)
                enemy.update()
                if level_time % 30 == 0:
                    enemy.shoot(random_number)
                player.enemy_collided(enemy)

            for building in building_list:
                building.draw(canvas)
                building.update()
                player.building_collided(building)

            for building in round_build_list:
                building.draw(canvas)
                building.update()
                pos = [building.pos[0], building.pos[1]]
                player.r_building_collided(building, pos)

            for bullet in ene_bullet_list:
                bullet.draw(canvas)
                bullet.update()
                player.bullet_collided(bullet)

            for bullet in bullet_list:
                bullet.draw(canvas)
                bullet.update()

                # These operations can't be in a class because they remove both the object and the bullet
                for enemy in enemy_list:
                    if round_dist(bullet.pos, enemy.pos) < enemy.radius:
                        enemy.shield -= 1
                        bullet_list.remove(bullet)
                        if enemy.shield <= 0:
                            points += enemy.score
                            enemy_list.remove(enemy)
                        else:
                            enemy.impact = True
                            enemy.first_impact = True
                        return
				
                for building in building_list:
                    if rect_dist(bullet, building.pos) < 1:
                        building.shield -= 1
                        bullet_list.remove(bullet)
                        if building.shield <= 0:
                            points += building.score
                            building_list.remove(building)
                        else:
                            building.impact = True
                            building.first_impact = True
                        return
				
                for building in round_build_list:
                    if round_dist(bullet.pos, building.pos) < building.radius:
                        if building.ID < 1000:  # not a ring
                            points += building.score
                            round_build_list.remove(building)
                            bullet_list.remove(bullet)
                        return
				
                if boss_fight:  # granga / mecha turret
                    if level < 4:
                        boss.bullet_collided(bullet)
                    else:  # team star wolf / andross
                        for boss in boss_list:
                            boss.bullet_collided(bullet)

        elif boss_defeat:
            player.draw(canvas, level_time)
            player.update()
            canvas.draw_text("ALL-RANGE MODE ACTIVE", [122, 790], 20, "white", "sans-serif")

            if level_time >= end_time - boss_text_time * 60:  # displays boss post-defeat transmission
                canvas.draw_text("TRANSMISSION", [156, 95], 25, "white", "sans-serif")
                for i in range(len(boss_text_end)):
                    if i % 2 == 0:
                        canvas.draw_text(boss_text_end[i], boss_text_end[i + 1], 25, "white", "sans-serif")
            elif level < 5:
                canvas.draw_text("Congratulations!", [140, 95], 30, "white", "sans-serif")
                canvas.draw_text("Points: " + str(points), [185, 150], 25, "white", "sans-serif")
                level_back.draw(canvas)
            elif level == 5:
                canvas.draw_text("Congratulations!", [140, 95], 30, "#22ff22", "sans-serif")
                canvas.draw_text("You beat the game!", [143, 150], 25, "white", "sans-serif")
                canvas.draw_text("Points: " + str(points), [185, 200], 25, "white", "sans-serif")
                level_back.draw(canvas)
                
                canvas.draw_text("Andross has been defeated! Thanks to you, Corneria is", [6, 300], 20, "white", "sans-serif")
                canvas.draw_text("safe. While evil may rise again, you have brought", [32, 325], 20, "white", "sans-serif")
                canvas.draw_text("peace, freedom, justice, and security to", [77, 350], 20, "white", "sans-serif")
                canvas.draw_text("your new empire I mean, Corneria. ;)", [88, 375], 20, "white", "sans-serif")
                canvas.draw_line([86, 369], [238, 369], 3, "white")  # draws strikethrough for "your new empire"


# spawn() and despawn() are called once per second to minimize lag.
def spawn():
    global level_completed, level, boss_fight
    global load_time, loading, loading_timer

    if level_time <= 1 and not boss_fight:  # timer is less than 1, all spawning has finished
        boss_fight = True
        loading = True
        load_time = 2
        loading_timer = level_time

    else:  # spawning still ongoing
        # Spawn lists are 2D lists:
        # [x][0] = time needed to pass since last spawn, [x][1] = ID value for preset building, [x][2] for position
        if len(b_spawn_list) != 0:
            b_time = b_spawn_list[0][0] * 60
            if b_time >= level_time:  # if specified time to spawn building in has been reached
                """
                For reference for square buildings: position, velocity, colour, ID, score, shield (defaults to 1)
                satellite velocity is generated in building class
                """
                if ID_dict["satellite"] <= b_spawn_list[0][1] <= ID_dict["satellite2"]:
                    if b_spawn_list[0][1] == ID_dict["satellite"]:
                        satellite = SquareBuilding([[100, -55], [100, 0], [155, 0], [155, -55]], [0, 5],
                                    'green', ID_dict["satellite"], 1)
                        building_list.append(satellite)
                    elif b_spawn_list[0][1] == ID_dict["satellite1"]:
                        satellite = SquareBuilding([[220, -55], [220, 0], [275, 0], [275, -55]], [0, 5],
                                    'green', ID_dict["satellite1"], 1)
                        building_list.append(satellite)
                    elif b_spawn_list[0][1] == ID_dict["satellite2"]:
                        satellite = SquareBuilding([[350, -55], [350, 0], [405, 0], [405, -55]], [0, 5],
                                    'green', ID_dict["satellite2"], 1)
                        building_list.append(satellite)

                elif ID_dict["turret"] <= b_spawn_list[0][1] <= ID_dict["turret2"]:
                    if b_spawn_list[0][1] == ID_dict["turret"]:
                        turret = SquareBuilding([[120, -55], [120, 0], [175, 0], [175, -55]], [0, 5],
                                    'green', ID_dict["turret"], 1, 3)
                        building_list.append(turret)
                    elif b_spawn_list[0][1] == ID_dict["turret1"]:
                        turret = SquareBuilding([[225, -55], [225, 0], [280, 0], [280, -55]], [0, 5],
                                    'green', ID_dict["turret1"], 1, 3)
                        building_list.append(turret)
                    elif b_spawn_list[0][1] == ID_dict["turret2"]:
                        turret = SquareBuilding([[345, -55], [345, 0], [400, 0], [400, -55]], [0, 5],
                                    'green', ID_dict["turret2"], 1, 3)
                        building_list.append(turret)

                b_spawn_list.pop(0)

        if len(rb_spawn_list) != 0:
            rb_time = rb_spawn_list[0][0] * 60
            if rb_time >= level_time:
                """
                For reference for round buildings: position, velocity, radius, colour, ID, score
                Debris velocity is generated inside round building class
                """
                if ID_dict["asteroid"] <= rb_spawn_list[0][1] <= ID_dict["small_asteroid1"]:
                    if rb_spawn_list[0][1] == ID_dict["asteroid"]:
                        asteroid = RoundBuilding([250, -30], [0, 5], 25, 'Green', ID_dict["asteroid"], 1)
                        round_build_list.append(asteroid)
                    elif rb_spawn_list[0][1] == ID_dict["asteroid1"]:
                        asteroid = RoundBuilding([400, -30], [0, 5], 25, 'Green', ID_dict["asteroid1"], 1)
                        round_build_list.append(asteroid)
                    elif rb_spawn_list[0][1] == ID_dict["asteroid2"]:
                        asteroid = RoundBuilding([100, -30], [0, 5], 25, 'Green', ID_dict["asteroid2"], 1)
                        round_build_list.append(asteroid)
                    elif rb_spawn_list[0][1] == ID_dict["small_asteroid"]:
                        small_asteroid = RoundBuilding([200, -20], [0, 5], 15, 'Green', ID_dict["small_asteroid"], 1)
                        round_build_list.append(small_asteroid)
                    elif rb_spawn_list[0][1] == ID_dict["small_asteroid1"]:
                        small_asteroid = RoundBuilding([350, -20], [0, 5], 15, 'Green', ID_dict["small_asteroid1"], 1)
                        round_build_list.append(small_asteroid)

                elif ID_dict["debris"] <= rb_spawn_list[0][1] <= ID_dict["debris2"]:
                    if rb_spawn_list[0][1] == ID_dict["debris"]:
                        debris = RoundBuilding([150, -30], [0, 0], 20, 'green', ID_dict["debris"], 1)
                        round_build_list.append(debris)
                    elif rb_spawn_list[0][1] == ID_dict["debris1"]:
                        debris = RoundBuilding([300, -30], [0, 0], 20, 'green', ID_dict["debris1"], 1)
                        round_build_list.append(debris)
                    elif rb_spawn_list[0][1] == ID_dict["debris2"]:
                        debris = RoundBuilding([425, -30], [0, 0], 20, 'green', ID_dict["debris2"], 1)
                        round_build_list.append(debris)

                elif ID_dict["ring"] <= rb_spawn_list[0][1] <= ID_dict["re_ring2"]:
                    # position, velocity, radius, colour, ID, score, health (that ring restores), max_increase (default 0)
                    # max_increase specifies by how much to increase max_shield
                    if rb_spawn_list[0][1] == ID_dict["ring"]:
                        ring = Ring([400, -10], [0, 9], 15, "white", ID_dict["ring"], 10, 15)
                        round_build_list.append(ring)
                    elif rb_spawn_list[0][1] == ID_dict["ring1"]:
                        ring = Ring([250, -10], [0, 9], 15, "white", ID_dict["ring1"], 10, 15)
                        round_build_list.append(ring)
                    elif rb_spawn_list[0][1] == ID_dict["ring2"]:
                        ring = Ring([100, -10], [0, 9], 15, "white", ID_dict["ring2"], 10, 15)
                        round_build_list.append(ring)

                    elif rb_spawn_list[0][1] == ID_dict["yl_ring"]:
                        ring = Ring([100, -10], [0, 9], 15, "yellow", ID_dict["yl_ring"], 10, 20, 10)
                        round_build_list.append(ring)
                    elif rb_spawn_list[0][1] == ID_dict["yl_ring1"]:
                        ring = Ring([250, -10], [0, 9], 15, "yellow", ID_dict["yl_ring1"], 10, 20, 10)
                        round_build_list.append(ring)
                    elif rb_spawn_list[0][1] == ID_dict["yl_ring2"]:
                        ring = Ring([400, -10], [0, 9], 15, "yellow", ID_dict["yl_ring2"], 10, 20, 10)
                        round_build_list.append(ring)

                    elif rb_spawn_list[0][1] == ID_dict["gr_ring"]:
                        ring = Ring([125, -10], [0, 9], 15, "#22ff22", ID_dict["gr_ring"], 10, 40, 10)
                        round_build_list.append(ring)
                    elif rb_spawn_list[0][1] == ID_dict["gr_ring1"]:
                        ring = Ring([250, -10], [0, 9], 15, "#22ff22", ID_dict["gr_ring1"], 10, 40, 10)
                        round_build_list.append(ring)
                    elif rb_spawn_list[0][1] == ID_dict["gr_ring2"]:
                        ring = Ring([375, -10], [0, 9], 15, "#22ff22", ID_dict["gr_ring2"], 10, 40, 10)
                        round_build_list.append(ring)

                    elif rb_spawn_list[0][1] == ID_dict["re_ring"]:
                        ring = Ring([140, -10], [0, 9], 15, "#ff1111", ID_dict["re_ring"], 10, player.max_shield, 5)
                        round_build_list.append(ring)
                    elif rb_spawn_list[0][1] == ID_dict["re_ring1"]:
                        ring = Ring([250, -10], [0, 9], 15, "#ff1111", ID_dict["re_ring1"], 10, player.max_shield, 5)
                        round_build_list.append(ring)
                    elif rb_spawn_list[0][1] == ID_dict["re_ring2"]:
                        ring = Ring([360, -10], [0, 9], 15, "#ff1111", ID_dict["re_ring2"], 10, player.max_shield, 5)
                        round_build_list.append(ring)

                rb_spawn_list.pop(0)

        if len(e_spawn_list) != 0:
            e_time = e_spawn_list[0][0] * 60
            if e_time >= level_time:
                # position, velocity, radius, colour, ID, score, shield (defaults to 2)
                if e_spawn_list[0][1] == ID_dict["fly"]:
                    fly = Enemy([250, -10], [5, 8], 20, "#ff1111", ID_dict["fly"], 5)
                    enemy_list.append(fly)

                elif ID_dict["mosquito"] <= e_spawn_list[0][1] <= ID_dict["mosquito2"]:
                    if e_spawn_list[0][1] == ID_dict["mosquito"]:
                        mosquito = Enemy([125, -10], [5, 8], 24, "#ff1111", ID_dict["mosquito"], 5)
                        enemy_list.append(mosquito)
                    elif e_spawn_list[0][1] == ID_dict["mosquito1"]:
                        mosquito = Enemy([250, -10], [5, 8], 24, "#ff1111", ID_dict["mosquito1"], 5)
                        enemy_list.append(mosquito)
                    elif e_spawn_list[0][1] == ID_dict["mosquito2"]:
                        mosquito = Enemy([375, -10], [5, 8], 24, "#ff1111", ID_dict["mosquito2"], 5)
                        enemy_list.append(mosquito)

                elif e_spawn_list[0][1] == ID_dict["hornet"]:
                    # 3 spawn at once
                    hornet = Enemy([100, -10], [3, 8], 25, "#ff1111", ID_dict["hornet"], 5)
                    hornet1 = Enemy([250, -10], [3, 8], 25, "#ff1111", ID_dict["hornet"], 5)
                    hornet2 = Enemy([400, -10], [3, 8], 25, "#ff1111", ID_dict["hornet"], 5)
                    enemy_list.append(hornet)
                    enemy_list.append(hornet1)
                    enemy_list.append(hornet2)

                elif ID_dict["queen_fly"] <= e_spawn_list[0][1] <= ID_dict["queen_fly2"]:
                    if e_spawn_list[0][1] == ID_dict["queen_fly"]:
                        queen = Enemy([125, -10], [5, 8], 30, "#ffff00", ID_dict["queen_fly"], 5, 4)
                        enemy_list.append(queen)
                    elif e_spawn_list[0][1] == ID_dict["queen_fly1"]:
                        queen = Enemy([250, -10], [5, 8], 30, "#ffff00", ID_dict["queen_fly1"], 5, 4)
                        enemy_list.append(queen)
                    elif e_spawn_list[0][1] == ID_dict["queen_fly2"]:
                        queen = Enemy([375, -10], [5, 8], 30, "#ffff00", ID_dict["queen_fly2"], 5, 4)
                        enemy_list.append(queen)

                elif e_spawn_list[0][1] == ID_dict["mini_andross"]:
                    mini = Enemy([250, 0], [5, 5], 32, "#ff1111", ID_dict["mini_andross"], 30, 15)
                    enemy_list.append(mini)

                e_spawn_list.pop(0)


def despawn():
    for enemy in enemy_list:
        if enemy.pos[1] > 820 or enemy.pos[1] < -20:
            enemy_list.remove(enemy)
        elif enemy.pos[0] > 520 or enemy.pos[0] < -20:
            enemy_list.remove(enemy)

    for building in building_list:
        if building.pos[0][1] > 820:
            building_list.remove(building)

    for building in round_build_list:
        if building.pos[1] > 820:
            round_build_list.remove(building)

    for bullet in bullet_list:
        if bullet.pos[1] > 810 or bullet.pos[1] < -10:
            bullet_list.remove(bullet)
        elif bullet.pos[0] > 510 or bullet.pos[0] < -10:
            bullet_list.remove(bullet)

    for bullet in ene_bullet_list:
        if bullet.pos[1] > 810 or bullet.pos[1] < -10:
            ene_bullet_list.remove(bullet)
        elif bullet.pos[0] > 510 or bullet.pos[0] < -10:
            ene_bullet_list.remove(bullet)

    for bullet in boss_bullet_list:
        if bullet.pos[1] > 810 or bullet.pos[1] < -10:
            boss_bullet_list.remove(bullet)
        elif bullet.pos[0] > 510 or bullet.pos[0] < -10:
            boss_bullet_list.remove(bullet)


# Calculates the velocity (speed and direction) of the bullet being fired.
def bullet_target(pos, dest_pos, vel):
    dest_x = dest_pos[0]
    dest_y = dest_pos[1]
    start_x = pos[0]
    start_y = pos[1]

    # controls which quadrant (relative to start) bullet travels in/towards
    negative_x = 1
    negative_y = 1

    if dest_y < start_y:
        negative_y = -1
    if dest_x < start_x:
        negative_x = -1

    # calculating the angle the bullet travels at
    y = math.fabs(dest_y - start_y)
    x = math.fabs(dest_x - start_x)
    angle = math.atan2(y, x)

    # original velocity is input; calculating components (s/o to physics 12)
    bullet_vel = [0, 0]
    bullet_vel[0] = vel * math.cos(angle) * negative_x
    bullet_vel[1] = vel * math.sin(angle) * negative_y

    return bullet_vel


# Handlers for key presses and releases.
def key_down(key):
    global moving_left
    global moving_right
    global moving_up
    global moving_down
    if WASD:
        if key == simplegui.KEY_MAP['A']:
            moving_left = True
        if key == simplegui.KEY_MAP['D']:
            moving_right = True
        if key == simplegui.KEY_MAP['W']:
            moving_up = True
        if key == simplegui.KEY_MAP['S']:
            moving_down = True

    else:  # control scheme set to arrow keys
        if key == simplegui.KEY_MAP['left']:
            moving_left = True
        if key == simplegui.KEY_MAP['right']:
            moving_right = True
        if key == simplegui.KEY_MAP['up']:
            moving_up = True
        if key == simplegui.KEY_MAP['down']:
            moving_down = True


def key_up(key):
    global moving_left
    global moving_right
    global moving_up
    global moving_down
    if WASD:
        if key == simplegui.KEY_MAP['A']:
            moving_left = False
        if key == simplegui.KEY_MAP['D']:
            moving_right = False
        if key == simplegui.KEY_MAP['W']:
            moving_up = False
        if key == simplegui.KEY_MAP['S']:
            moving_down = False

    else:  # control scheme set to arrow keys
        if key == simplegui.KEY_MAP['left']:
            moving_left = False
        if key == simplegui.KEY_MAP['right']:
            moving_right = False
        if key == simplegui.KEY_MAP['up']:
            moving_up = False
        if key == simplegui.KEY_MAP['down']:
            moving_down = False


# Handler for mouse clicks; code to fire player bullets is here.
def mouse_handler(position):
    global level
    global WASD
    # Opening menus and starting games while in the menus
    if level == 0:  # main menu
        if level_select.click(position):
            level = level_select.level
        elif help_button.click(position):
            level = help_button.level
        elif options.click(position):
            level = options.level

    elif level == -1:  # level select menu
        if back.click(position):
            level = back.level
        elif intro.click(position):
            level = intro.level
        elif level1.click(position):
            level = level1.level
            new_game()
        elif level2.click(position) and level2.on:
            level = level2.level
            new_game()
        elif level3.click(position) and level3.on:
            level = level3.level
            new_game()
        elif level4.click(position) and level4.on:
            level = level4.level
            new_game()
        elif level5.click(position) and level5.on:
            level = level5.level
            new_game()

    elif level == -2:  # help menu
        if back.click(position):
            level = back.level

    elif level == -3:  # options menu
        if back.click(position):
            level = back.level
        elif wasd_button.click(position):
            WASD = True
            wasd_button.on = True
            arrow_button.on = False
        elif arrow_button.click(position):
            WASD = False
            wasd_button.on = False
            arrow_button.on = True

    elif level == -4:  # background story
        if level_back.click(position):
            level = level_back.level
            
    elif level > 0:  # in game
        if player.dead:
            if level_back.click(position):
                level = level_back.level

        elif not boss_defeat and not loading:  # fire player bullets
            # starting pos, dest pos, vel
            bullet_vel = bullet_target(player.pos, position, 10)

            # position, velocity, radius, colour, ID
            if position[1] <= player.pos[1]:
                bullet = Bullet([player.pos[0], (player.pos[1] - player.radius)], bullet_vel, 2, "white", -1)
            else:
                bullet = Bullet([player.pos[0], (player.pos[1] + player.radius)], bullet_vel, 2, "white", -1)
            bullet_list.append(bullet)

        elif boss_defeat:
            if level_back.click(position):
                if level_completed == 1:
                    level2.on = True
                elif level_completed == 2:
                    level3.on = True
                elif level_completed == 3:
                    level4.on = True
                elif level_completed == 4:
                    level5.on = True
                level = level_back.level


# Initializing the frame and setting handlers
frame = simplegui.create_frame("Star Fox 2D v1.1", FRAME_WIDTH, FRAME_HEIGHT)
frame.set_draw_handler(draw)
frame.set_keydown_handler(key_down)
frame.set_keyup_handler(key_up)
frame.set_mouseclick_handler(mouse_handler)

start()
# new_game() is being called in mouse handler on level start.
frame.start()