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
class MovingBoard{
    int[,] board;
    Random rand=new Random();
    int dim;
    const int EMPTY=0;
    bool gameOver=false;
    int[,] prevBoard;
    int score=0;
    int prevScore=0;
    List<(int val,int x1,int y1,int x2,int y2,int dx1,int dy1)> forAnime=
    new List<(int val,int x1,int y1,int x2,int y2,int dx1,int dy1)>();
    (int ver,int hor)[] dirs={(1,0),    // down
                              (0,1),    // right
                              (-1,0),   // up
                              (0,-1)};  // left
    public MovingBoard(int n){
        board=new int[n,n];
        prevBoard=new int[n,n];
        dim=n;
        initBoard();
    }
    void initBoard(){
        for(int i=0;i<dim;i++){
            for(int j=0;j<dim;j++){
                board[i,j]=EMPTY;
                prevBoard[i,j]=EMPTY;
            }
        }
        int x=rand.Next(dim);
        int y=rand.Next(dim);
        board[x,y]=2;
        prevBoard[x,y]=2;
    }
    public bool getGameState()=>gameOver;
    public int getTile(int i,int j)=> board[i,j];
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
    public List<(int val,int x1,int y1,int x2,int y2,int dx1,int dy1)> getAnime(){
        return forAnime;
    }
    bool permitted(){
        for(int i=0;i<dim;i++){
            for(int j=0;j<dim;j++){
                if(board[i,j]!=prevBoard[i,j]){
                    return true;
                }
            }
        }
        return false;
    }
    public void initAnime(){
        forAnime=new List<(int val,int x1,int y1,int x2,int y2,int dx1,int dy1)>();
    }
    (int x,int y,int z,int dz) mapping(int i,int j,Direction dir){
        if(dir==Up)return(j,i,j,1);             // up
        if(dir==Down)return(dim-j-1,dim-i-1,dim-j-1,-1);   // down
        if(dir==Left)return(i,j,j,1);           // left
        if(dir==Right)return(dim-i-1,dim-j-1,dim-j-1,-1);  // right

        return(0,0,0,0);
    }
    (bool,bool) moveEmptyDir(Direction dir,int x,int y,int z,int dz,bool moved,bool isFirst){
        int k=y;
        int val=EMPTY;
        if(dir==Up || dir ==Down){
            while(true){
                val=board[z,y];
                if(val!=EMPTY){
                    moved=true;
                    if(isFirst){
                        updatePrevStep();
                        isFirst=!isFirst;
                    }
                    board[z,y]=EMPTY;
                    forAnime.Add((val,z,y,x,y,-dz,0));
                    break;
                }
                z+=dz;
                if(z>dim-1 || z<0){
                    break;
                }
            }
        }
        if(dir==Right || dir ==Left){
            while(true){
                val=board[x,z];
                if(val!=EMPTY){
                    moved=true;
                    if(isFirst){
                        updatePrevStep();
                        isFirst=!isFirst;
                    }
                    board[x,z]=EMPTY;
                    forAnime.Add((val,x,z,x,y,0,-dz));
                    break;
                }
                z+=dz;
                if(z>dim-1 || z<0){
                    break;
                }
            }
        }
        board[x,y]=val;
        return (moved,isFirst);
    }
    (bool,bool) moveNonEmptyDir(Direction dir,int x,int y,int z,int dz,bool moved,bool isFirst){
        int val=EMPTY;
        int value=board[x,y];
        if(dir==Up || dir ==Down){
            while(true){
                z+=dz;
                if(z>dim-1 || z<0){
                    break;
                }
                val=board[z,y];
                if(val==value){
                    moved=true;
                    if(isFirst){
                        updatePrevStep();
                        isFirst=!isFirst;
                    }
                    board[x,y]=2*val;
                    // for animation
                    forAnime.Add((val,z,y,x,y,-dz,0));
                    board[z,y]=EMPTY;
                    prevScore=score;
                    score+=2*val;
                    break;
                }
                else if(val!=EMPTY) break;
            }
        }
        if(dir==Right || dir ==Left){
            while(true){
                z+=dz;
                if(z>dim-1 || z<0){
                    break;
                }
                val=board[x,z];
                if(val==value){
                    moved=true;
                    if(isFirst){
                        updatePrevStep();
                        isFirst=!isFirst;
                    }
                    board[x,y]=2*val;
                    // for animation
                    forAnime.Add((val,x,z,x,y,0,-dz));
                    board[x,z]=EMPTY;
                    prevScore=score;
                    score+=2*val;
                    break;
                }
                if(val!=EMPTY) break;
            }
        }
        return (moved,isFirst);
    }
    
    bool moveDir(Direction dir){
        if(dir==Stay){
            return false;
        }
        bool moved=false;
        bool isFirst=true;
        for(int i=0;i<dim;i++){ 
            for(int j=0;j<dim;j++){
                (int x,int y,int z,int dz)=mapping(i,j,dir);
                // Case 1: the value is empty
                if(board[x,y]==EMPTY)(moved,isFirst)=moveEmptyDir(dir,x,y,z,dz,moved,isFirst);
                // Case 2: the value is not empty
                if(board[x,y]!=EMPTY)(moved,isFirst)=moveNonEmptyDir(dir,x,y,z,dz,moved,isFirst);
            }
        }
        return moved;
    }
    (int,int) newPos(){
        int nullCount=0;
        for(int i=0;i<dim;i++){
            for(int j=0;j<dim;j++){
                if(board[i,j]==EMPTY){
                    nullCount++;
                }
            }
        }
        int pos=rand.Next(nullCount);
        for(int i=0;i<dim;i++){
            for(int j=0;j<dim;j++){
                if(board[i,j]==EMPTY){
                    pos--;
                    if(pos==-1){
                        return (i,j);
                    }
                }
            }
        }
        return (0,0);
    }
    void updatePrevStep(){
        for(int i=0;i<dim;i++){
            for(int j=0;j<dim;j++){
                prevBoard[i,j]=board[i,j];
            }
        }
        prevScore=score;
    }
    void goBack(){
        for(int i=0;i<dim;i++){
            for(int j=0;j<dim;j++){
                board[i,j]=prevBoard[i,j];
            }
        }
        score=prevScore;
    }
    bool hasEqualNeighbor(int i,int j){
        return  (i>0 && board[i-1,j]==board[i,j]) 
        ||      (i<dim-1 && board[i+1,j]==board[i,j]) 
        ||      (j>0 && board[i,j-1]==board[i,j]) 
        ||      (j<dim-1 && board[i,j+1]==board[i,j]);
    }
    // checking whether the game is over or not
    void over(){
        // if there is a empty tile or 
        // there are two neighbor tiles that
        // have the same value
        // then game is not over
        for(int i=0;i<dim;i++){
            for(int j=0;j<dim;j++){
                if(board[i,j]==0 || hasEqualNeighbor(i,j)){
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
    public List<(int val,int x1,int y1,int x2,int y2,int dx1,int dy1)> forAnime=
    new List<(int val,int x1,int y1,int x2,int y2,int dx1,int dy1)>();
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
    double timer=5,dt=0.5;
    public View(){
        AddEvents((int)EventMask.KeyPressMask);
        AddEvents((int)EventMask.ButtonPressMask);
    }
    protected override bool OnDrawn (Context c) {
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
            for(int i=0;i<size;i++){
                for(int j=0;j<size;j++){
                    c.SetSourceColor(colors[game.getTile(i,j)]);
                    c.Rectangle(x: 10+j*sizes[size].l, y: 160+i*sizes[size].l, width: sizes[size].d, height: sizes[size].d);
                    c.Fill();
                    if(game.getTile(i,j)>0){
                        c.SetSourceColor(white);
                        centerText(c,10+sizes[size].d/2+sizes[size].l*j,160+sizes[size].d/2+sizes[size].l*i,
                                                                sizes[size].font,game.getTile(i,j).ToString());
                    }
                }
            }
            if(animation){
                animation=false;
                for(int i=0;i<forAnime.Count;i++){
                    (int val,int x1,int y1,int x2,int y2,int dx1,int dy1) elem=forAnime[i];
                    if(!(elem.x1==elem.x2 && elem.y1==elem.y2)){
                        c.SetSourceColor(colors[forAnime[i].val]);
                        c.Rectangle(x: elem.y1, y: elem.x1, width: sizes[size].d, height: sizes[size].d);
                        c.Fill();
                        if(((elem.x1+elem.dx1-elem.x2)*(elem.x1-elem.x2)<0) || 
                           ((elem.y1+elem.dy1-elem.y2)*(elem.y1-elem.y2))<0){
                               forAnime[i]=(elem.val,elem.x2,elem.y2,elem.x2,elem.y2,elem.dx1,elem.dy1);
                            }
                        else{
                            forAnime[i]=(elem.val,elem.x1+elem.dx1,elem.y1+elem.dy1,elem.x2,elem.y2,elem.dx1,elem.dy1);
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
        int n=forAnime.Count;
        while(n-1>-1){
            forAnime.RemoveAt(n-1);
            n--;
        }
    }
    public void animate(){
        int animSpeed=40;
        var forAnime1=game.getAnime();
        for(int i=0;i<forAnime1.Count;i++){
            (int val,int x1,int y1,
            int x2,int y2,int dx1,int dy1)elem=forAnime1[i];
            forAnime.Add((elem.val,160+elem.x1*sizes[size].l,10+forAnime1[i].y1*sizes[size].l,
                                160+elem.x2*sizes[size].l,10+elem.y2*sizes[size].l,
                                animSpeed*elem.dx1,animSpeed*elem.dy1));
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
    protected override bool OnButtonPressEvent (EventButton e) {
        (double x,double y)= (e.X, e.Y);
        if(menu){
            if(x<=320 && x>=130 && y<=240 && y>=180){
                game=new MovingBoard(3);
                timer=5;
                size=3;
                menu=!menu;
            }
            if(x<=320 && x>=130 && y<=330 && y>=270){
                game=new MovingBoard(4);
                timer=5;
                size=4;
                menu=!menu;
            }
            if(x<=320 && x>=130 && y<=420 && y>=360){
                game=new MovingBoard(5);
                timer=5;
                size=5;
                menu=!menu;
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
        if (e.Key == Key.Left){
            view.move(Left);
            view.resetTimer();
            view.cleanAnime();
            view.animate();
        }
        else if (e.Key == Key.Right){
            view.move(Right);
            view.resetTimer();
            view.cleanAnime();
            view.animate();
        }
        else if (e.Key == Key.Up){
            view.move(Up);
            view.resetTimer();
            view.cleanAnime();
            view.animate();
        }
        else if (e.Key == Key.Down){
            view.move(Down);
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
