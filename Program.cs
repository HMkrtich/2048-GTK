using Cairo;
using Gdk;
using Gtk;
using static Gdk.EventMask;
using Color = Cairo.Color;
using static System.Console;
using System.Collections.Generic;
using System;
using Key = Gdk.Key;
using static Direction;
using Timeout = GLib.Timeout;
enum Direction{
    Right,
    Left,
    Up,
    Down,
    Stay
};
record AnimeRecord(int val, int x1, int y1, int x2, int y2, int dx1, int dy1) {
}
class MovingBoard{
    int[,] board;
    Random rand=new Random();
    int dim;
    const int EMPTY=0;
    bool gameOver=false;
    int[,] prevBoard;
    int score=0;
    int prevScore=0;
    List<AnimeRecord> forAnime=new List<AnimeRecord>();
    public MovingBoard(int n){
        board=new int[n,n];
        prevBoard=new int[n,n];
        dim=n;
        initBoard();
    }
    void initBoard(){
        for(int x=0;x<dim;x++){
            for(int y=0;y<dim;y++){
                board[x,y]=EMPTY;
                prevBoard[x,y]=EMPTY;
            }
        }
        int x_=rand.Next(dim);
        int y_=rand.Next(dim);
        board[x_,y_]=2;
        prevBoard[x_,y_]=2;
    }
    public bool getGameState()=>gameOver;
    public int getTile(int x,int y)=> board[x,y];
    public int getScore()=>score;
    public void repeat(){
        if(permitted())
            goBack();
    }
    public Direction getRandDirection(){
        Direction[] dirs={Up,Down,Left,Right};
        return dirs[rand.Next(4)];
    }
    public void restart(){
        initBoard();
        score=0;
        prevScore=0;
        gameOver=false;
    }
    public List<AnimeRecord> getAnime(){
        return forAnime;
    }
    bool permitted(){
        for(int x=0;x<dim;x++){
            for(int y=0;y<dim;y++){
                if(board[x,y]!=prevBoard[x,y]){
                    return true;
                }
            }
        }
        return false;
    }
    public void initAnime(){
        forAnime=new List<AnimeRecord>();
    }
    (int,int) mapping1(int x,int y,Direction dir){
        if(dir==Up)return(y,x);             // up
        if(dir==Down)return(dim-y-1,dim-x-1);   // down
        if(dir==Left)return(x,y);           // left
        if(dir==Right)return(dim-x-1,dim-y-1);  // right
        throw new Exception("invalid direction");
    }
    (int,int) mapping2(int x,int y,Direction dir){
        if(dir==Up) return(0,1);
        if(dir==Down)return(0,-1);
        if(dir==Left) return(1,0);
        if(dir==Right) return(-1,0);
        throw new Exception("invalid direction");
    }
    (bool,bool) moveEmptyDir(Direction dir,int x,int y,bool moved,bool isFirst){
        int val=EMPTY;
        (int x1,int y1)=(x,y);
        (int dx1,int dy1)=mapping2(x,y,dir);
        while(true){
            val=board[x1,y1];
            if(val!=EMPTY){
                moved=true;
                if(isFirst){
                    updatePrevStep();
                    isFirst=!isFirst;
                }
                board[x1,y1]=EMPTY;
                forAnime.Add(new (val,x1,y1,x,y,-dx1,-dy1));
                break;
            }
            x1+=dx1;
            y1+=dy1;
            if((y1>dim-1 || y1<0)||(x1>dim-1 || x1<0)){
                break;
            }
        }
        board[x,y]=val;
        return (moved,isFirst);
    }
    (bool,bool) moveNonEmptyDir(Direction dir,int x,int y,bool moved,bool isFirst){
        int val=EMPTY;
        int value=board[x,y];
        (int x1,int y1)=(x, y);
        (int dx1,int dy1)=mapping2(x,y,dir);
         while(true){
            y1+=dy1;
            x1+=dx1;
            if(x1>dim-1 || x1<0){
                break;
            }
            if((y1>dim-1 || y1<0)||(x1>dim-1 || x1<0)){
                break;
            }
            val=board[x1,y1];
            if(val==value){
                moved=true;
                if(isFirst){
                    updatePrevStep();
                    isFirst=!isFirst;
                }
                board[x,y]=2*val;
                // for animation
                forAnime.Add(new (val,x1,y1,x,y,-dx1,-dy1));
                board[x1,y1]=EMPTY;
                prevScore=score;
                score+=2*val;
                break;
            }
            else if(val!=EMPTY) break;
        }
        return (moved,isFirst);
    }
    
    bool moveDir(Direction dir){
        if(dir==Stay){
            return false;
        }
        bool moved=false;
        bool isFirst=true;
        for(int x=0;x<dim;x++){ 
            for(int y=0;y<dim;y++){
                (int x_,int y_)=mapping1(x,y,dir);
                // Case 1: the value is empty
                if(board[x_,y_]==EMPTY)(moved,isFirst)=moveEmptyDir(dir,x_,y_,moved,isFirst);
                // Case 2: the value is not empty
                if(board[x_,y_]!=EMPTY)(moved,isFirst)=moveNonEmptyDir(dir,x_,y_,moved,isFirst);
            }
        }
        return moved;
    }
    (int,int) newPos(){
        int nullCount=0;
        for(int x=0;x<dim;x++){ 
            for(int y=0;y<dim;y++){
                if(board[x,y]==EMPTY){
                    nullCount++;
                }
            }
        }
        int pos=rand.Next(nullCount);
        for(int x=0;x<dim;x++){ 
            for(int y=0;y<dim;y++){
                if(board[x,y]==EMPTY){
                    pos--;
                    if(pos==-1){
                        return (x,y);
                    }
                }
            }
        }
        return (0,0);
    }
    void updatePrevStep(){
        prevBoard = (int[,]) board.Clone();
        prevScore=score;
    }
    void goBack(){
        board = (int[,]) prevBoard.Clone();
        score=prevScore;
    }
    bool hasEqualNeighbor(int x,int y){
        return  (x>0 && board[x-1,y]==board[x,y]) 
        ||      (x<dim-1 && board[x+1,y]==board[x,y]) 
        ||      (y>0 && board[x,y-1]==board[x,y]) 
        ||      (y<dim-1 && board[x,y+1]==board[x,y]);
    }
    // checking whether the game is over or not
    void over(){
        // if there is a empty tile or 
        // there are two neighbor tiles that
        // have the same value
        // then game is not over
        for(int x=0;x<dim;x++){ 
            for(int y=0;y<dim;y++){
                if(board[x,y]==0 || hasEqualNeighbor(x,y)){
                    return;
                }
            }
        }
        gameOver=true;
    }
    public void play(Direction diri){        
        bool moved=moveDir(diri);
        if(moved){
            int num=rand.Next(1,3);
            int binPow=(int)Math.Pow(2,num);
            (int x,int y)=newPos();
            board[x,y]=binPow;
        }
        // Checking whether the game is Over or not
        over();  
    }
}


class View : DrawingArea {
    // The maximum number that you can get in "3 x 3" grid is 2^(3*3+1)
    //                                        "4 x 4" grid is 2^(4*4+1)
    //                                        "5 x 5" grid is 2^(5*5+1)
    //Colors of every kind of Tiles
    
    Dictionary<int,Color> colors=new Dictionary<int,Color>(){{0,color(230,230,250)},
        {2,color(255,215,0)},{4,color(255,165,0)},
        {8,color(255,140,0)},{16,color(255,69,0)},
        {32,color(255,160,122)},{64,color(250,128,114)},
        {128,color(233,150,122)},{256,color(240,128,128)},
        {512,color(205,92,92)},{1024,color(255,127,80)},
        {2048,color(255,99,71)},{4096,color(255,0,0)},
        {8192,color(220,20,60)},{16384,color(178,34,34)},
        {32768,color(165,42,42)},{65536,color(139,0,0)},
        {131072,color(128,0,0)},{262144,color(0,255,255)},
        {524288,color(0,255,255)},{1048576,color(0,139,139)},
        {2097152,color(0,128,128)},{4194304,color(47,79,79)},
        {8388608,color(25,25,112)},{16777216,color(139,0,139)},
        {33554432,color(112,128,144)},{67108864,color(0,0,0)}
    };
    // Specification of the fonts and margins of the "n x n" game 
    Dictionary<int,(int l,int d,int font)> sizes=new Dictionary<int,(int l,int d,int font)>()
                                            {{3,(145,135,35)},{4,(110,100,30)},{5,(87,77,20)}};
    List<AnimeRecord>forAnime=new List<AnimeRecord>();
    Color black = new Color(0, 0, 0),
          blue = new Color(0, 0, 1),
          light_green = new Color(0.56, 0.93, 0.56),
          grey=new Color(230.0/256,230.0/256,250.0/256),
          pink=new Color(216.0/256,191.0/256,216.0/256),
          white=new Color(1,1,1);

    MovingBoard game=new MovingBoard(4);
    int size; // dimension
    bool menu=true;
    bool animation=false;
    const int ANIM_SPEED=40;
    double timer=5,dt=0.5;
    public View(){
        AddEvents((int)EventMask.KeyPressMask);
        AddEvents((int)EventMask.ButtonPressMask);
    }
    protected override  bool OnDrawn (Context c) {
        if(menu){
            c.SetSourceColor(pink);
            c.Rectangle(x: 30, y: 30, width: 390, height: 540);
            c.Fill();
            c.SetSourceColor(color(230,230,250));
            c.Rectangle(x: 130, y: 180, width: 190, height: 60);
            c.Rectangle(x: 130, y: 270, width: 190, height: 60);
            c.Rectangle(x: 130, y: 360, width: 190, height: 60);
            c.Fill();
            c.SetSourceColor(blue); // Displaying Restart button
            centerText(c,225,210,25,"  Tiny     (3 X 3)");
            centerText(c,225,300,25,"Classic (4 X 4)");
            centerText(c,225,390,25,"     Big    (5 X 5)");
        } else{
            c.SetSourceColor(pink); // displaying score
            c.Rectangle(x: 10, y: 50, width: 100, height: 60);
            c.Rectangle(x: 120, y: 50, width: 100, height: 60);
            c.Rectangle(x: 230, y: 50, width: 100, height: 60);
            c.Rectangle(x: 340, y: 50, width: 100, height: 60);
            c.Rectangle(x: 120, y: 115, width: 210, height: 30);
            c.Rectangle(x: 0, y: 150, width: 450, height: 450);
            c.Fill();
            for(int x=0;x<size;x++){
                for(int y=0;y<size;y++){
                    c.SetSourceColor(colors[game.getTile(x,y)]);
                    c.Rectangle(x: 10+x*sizes[size].l, y: 160+y*sizes[size].l, width: sizes[size].d, height: sizes[size].d);
                    c.Fill();
                    if(game.getTile(x,y)>0){
                        c.SetSourceColor(white);
                        centerText(c,10+sizes[size].d/2+sizes[size].l*x,160+sizes[size].d/2+sizes[size].l*y,
                                                                sizes[size].font,game.getTile(x,y).ToString());
                    }
                }
            }
            if(animation){
                animation=false;
                for(int i=0;i<forAnime.Count;i++){
                    AnimeRecord elem=forAnime[i];
                    if(!(elem.x1==elem.x2 && elem.y1==elem.y2)){
                        c.SetSourceColor(colors[forAnime[i].val]);
                        c.Rectangle(x: elem.x1, y: elem.y1, width: sizes[size].d, height: sizes[size].d);
                        c.Fill();
                        if(((elem.x1+elem.dx1-elem.x2)*(elem.x1-elem.x2)<0) || 
                           ((elem.y1+elem.dy1-elem.y2)*(elem.y1-elem.y2))<0){
                               forAnime[i]=new (elem.val,elem.x2,elem.y2,elem.x2,elem.y2,elem.dx1,elem.dy1);
                            }
                        else{
                            forAnime[i]=new (elem.val,elem.x1+elem.dx1,elem.y1+elem.dy1,elem.x2,elem.y2,elem.dx1,elem.dy1);
                        }
                    }
                }
            }
            c.SetSourceColor(grey); // Displaying the Score
            centerText(c,390,60,15,"Score");
            centerText(c,390,85,20,game.getScore().ToString());
            centerText(c,390,60,15,"Score"); // Displaying the Timer
            centerText(c,225,130,20,String.Format("{0:0.0}", timer));
            c.SetSourceColor(light_green);  // Displaying Repeat button
            centerText(c,280,75,15,"REPEAT");
            c.SetSourceColor(light_green); // Displaying Restart button
            centerText(c,170,75,15,"RESTART");
            c.SetSourceColor(blue);// Displaying Restart button
            centerText(c,60,75,15,"MENU");
            if(game.getGameState()){  // Displaying the game over screen
                c.SetSourceColor(black);
                centerText(c,225,300,30,"Game Over!");
            }
        }
        return true;
    }
    public void cleanAnime(){
        forAnime.Clear();
    }
    public void animate(){
        var forAnime1=game.getAnime();
        for(int i=0;i<forAnime1.Count;i++){
            AnimeRecord elem=forAnime1[i];
            forAnime.Add(new (elem.val,10+forAnime1[i].x1*sizes[size].l,160+elem.y1*sizes[size].l,
                                10+elem.x2*sizes[size].l,160+elem.y2*sizes[size].l,
                                ANIM_SPEED*elem.dx1,ANIM_SPEED*elem.dy1));
        }
        game.initAnime();
    }
    public void setAnime(){
        animation=true;
    }
    static Color color(int r, int g, int b) => new Color(r / 255.0, g / 255.0, b / 255.0);
    static void centerText(Context c, int x, int y,int font, string s) {
        c.SetFontSize(font);
        TextExtents te = c.TextExtents(s);
        c.MoveTo(x - (te.Width / 2 + te.XBearing), y - (te.Height / 2 + te.YBearing));
        c.ShowText(s);        
    }
    void startGame(int s) {
        game=new MovingBoard(s);
        timer=5;
        size=s;
        menu=!menu;
    }

    protected override bool OnButtonPressEvent (EventButton e) {
        (double x,double y)= (e.X, e.Y);
        if(menu){
            if(x<=320 && x>=130){
                if(y<=240 && y>=180){
                    startGame(3);
                }
                else if(y<=330 && y>=270){
                    startGame(4);
                }
                else if(y<=420 && y>=360){
                    startGame(5);
                }
            }
        }
        else{
            if(!game.getGameState()){
                if(x<=330 && x>=230 && y<=110 && y>=50){
                    game.repeat();
                    timer=5;
                }
                if(x<=220 && x>=120 && y<=110 && y>=50){
                    game.restart();
                    timer=5;
                }
            }
            if(x<=110 && x>=10 && y<=110 && y>=50){
                menu=true;
                game.restart();
                timer=5;
            }
        }
        QueueDraw();
        return true;
    }
    void moveRandom(){
        move(game.getRandDirection());
    }
    public void move(Direction dir){
        game.play(dir);
    }
    public void resetTimer(){
        timer=5;
    }
    public void updateTime(){
        if(!game.getGameState()){
            timer-=dt;
            timer=Math.Round(timer, 2);
            if(timer<=0){
                moveRandom();
                cleanAnime();
                animate();
                timer=5;
            }
        }
    }
}
class MyWindow : Gtk.Window {
    View view=new View();
    public MyWindow() : base("2048") {
        Resize(450, 600);
        Add(view);  // add an Area to the window
        Timeout.Add(1, animation);
        Timeout.Add(500, on_timeout);
    }
     bool on_timeout() {
        view.updateTime();
        QueueDraw();
        return true;
    }
    bool animation(){
        view.setAnime();
        QueueDraw();
        return true;
    }
    protected override bool OnKeyPressEvent(EventKey e) {
        var dirs = new Dictionary<Key, Direction> {
            { Key.Left, Left}, {Key.Right, Right}, {Key.Up, Up}, {Key.Down, Down}
        };
        if (dirs.TryGetValue(e.Key, out Direction dir)) {
            view.move(dir);
            view.resetTimer();
            view.cleanAnime();
            view.animate();
        }
        QueueDraw();
        return true;
    }
    protected override bool OnDeleteEvent(Event e) {
        Application.Quit();
        return true;
    }
}
class Hello {
    static void Main() {
        Application.Init();
        MyWindow w = new MyWindow();
        w.ShowAll();
        Application.Run();
    }
}
