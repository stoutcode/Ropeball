using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using Jypeli.Effects;
using System.Diagnostics;

/// @author mimariep
/// @version 20.11.2019
/// <summary>
/// Ropeball pelissa seikkaillaan kosmisella pallolla ja kerätään tähtiä eri maailmoista.                                        
/// </summary>
public class Ropeball : PhysicsGame
{
    PhysicsObject pelaaja; // pelipallo
    PhysicsObject taso; // valkoinen aukko
    PhysicsObject taso2; // toinen valkoinen aukko
    PhysicsObject koysi; // koysi
    IntMeter keratytAarteet; // kerattavien aarteiden intmeter
    Label naytto; // naytto, jossa edellinen nakyy
    bool koydenOlemassaOlo; // koyden olemassaolo kerrotaan talle muuttujalle
    bool taso1OlemassaOlo; // valkoisen aukon olemassaolo kerrotaan talle
    bool taso2OlemassaOlo; // toisen valkoisen aukon olemassaolo kerrotaan talle
    bool rajoitin; // hyppyjen toistumisen rajoittamista varten oleva muuttuja
    int maailmaNro; // maailman vaihtumista maaraava muuttuja

    /// <summary>
    /// Tämä ohjelma kuvastaa kosmisen pallon seikkailua vieraassa maailmassa.
    /// Pallo pyrkii keräämään maailman tähdet ja siirtymään aina seuraavaan maailmaan.
    /// Maailmoja on kolme tällä hetkellä.
    /// </summary>
    public override void Begin()
    {
        SetWindowSize(1280, 960, false);
        switch (maailmaNro) // katsotaan mikä kenttä valitaan
        {
            case 0:
            case 1:
                maailmaNro = 1;
                PeliMaailma1();
                break;
            case 2:
                PeliMaailma2();
                break;
            case 3:
                PeliMaailma3();
                break;
        }
        Ohjaimet(); 
    }


    /// <summary>
    /// Luodaan ensimmäinen pelimaailma.
    /// </summary>
    private void PeliMaailma1()
    {
        LisaaPuitteet(); // lisätään kentille yhteiset puitteet
        taso = UusiTaso(this, 120, 250, 200);
        taso1OlemassaOlo = true;
        pelaaja = Pelaaja(this, 0, -200, 60);
        AddCollisionHandler(pelaaja, taso, OsuiTasoon);
        AddCollisionHandler(pelaaja, "aarre", LoysiAarteen);
        MediaPlayer.Play("shadydave_anticipation"); // taustamusiikki
        MediaPlayer.IsRepeating = true;
        MessageDisplay maailma1 = new MessageDisplay(); // maailman aloitukseen viesti
        maailma1.Add("Oo.. tähtiä.. tuo valkoinen aukko näyttää mystiseltä...", Color.Cyan);
        maailma1.Position = new Vector(300, 150);
        maailma1.BackgroundColor = Color.Transparent;
        maailma1.BorderColor = Color.Transparent;
        this.Add(maailma1);
    }


    /// <summary>
    /// Luodaan toinen pelimaailma
    /// </summary>
    private void PeliMaailma2()
    {
        LisaaPuitteet(); // lisätään kentille yhteiset puitteet
        taso = UusiTaso(this, 120, -500, 100);
        taso1OlemassaOlo = true;
        taso2 = UusiTaso(this, 120, 600, 300);
        taso2OlemassaOlo = true;
        pelaaja = Pelaaja(this, 0, -200, 60);
        AddCollisionHandler(pelaaja, "aarre", LoysiAarteen);
        AddCollisionHandler(pelaaja, taso, OsuiTasoon);
        AddCollisionHandler(pelaaja, taso2, OsuiTasoon);
        MediaPlayer.Play("shadydave_anticipation"); // taustamusiikki
        MediaPlayer.IsRepeating = true;
        MessageDisplay maailma2 = new MessageDisplay(); // maailman aloitukseen viesti
        maailma2.Position = new Vector(300, 150);
        maailma2.Add("Siitä pääsi läpi!", Color.Cyan);
        maailma2.BackgroundColor = Color.Transparent;
        maailma2.BorderColor = Color.Transparent;
        this.Add(maailma2);
    }


    /// <summary>
    /// Luodaan kolmas pelimaailma
    /// </summary>
    private void PeliMaailma3()
    {
        LisaaPuitteet(); // lisätään kentille yhteiset puitteet
        taso = UusiTaso(this, 120, -500, 50);
        taso1OlemassaOlo = true;
        taso2 = UusiTaso(this, 120, 600, 650);
        taso2OlemassaOlo = true;
        pelaaja = Pelaaja(this, 0, -200, 60);
        LisaaVaaroja(RandomGen.NextInt(4, 10));
        AddCollisionHandler(pelaaja, "aarre", LoysiAarteen);
        AddCollisionHandler(pelaaja, taso, OsuiTasoon);
        AddCollisionHandler(pelaaja, taso2, OsuiTasoon);
        AddCollisionHandler(pelaaja, "vaara", OsuiVaaraan);
        MediaPlayer.Play("shadydave_anticipation"); // taustamusiikki
        MediaPlayer.IsRepeating = true;
        MessageDisplay maailma3 = new MessageDisplay(); // maailman aloitukseen viesti
        maailma3.Position = new Vector(200, 200);
        maailma3.Add("Nuo punaiset tähdet vaikuttavat vaarallisilta...", Color.Cyan);
        maailma3.BackgroundColor = Color.Transparent;
        maailma3.BorderColor = Color.Transparent;
        this.Add(maailma3);
    }


    /// <summary>
    /// Lisätään eri pelimaailmoille yhteiset puitteet kenttiin
    /// </summary>
    private void LisaaPuitteet()
    {
        Level.Width = 3000;
        Level.Height = 2250;
        Camera.ZoomToLevel();
        Level.CreateBorders(1, true);
        Level.Background.CreateStars(1000); // tähtiä taustakuvaksi
        Gravity = new Vector(0, -900);
        LisaaMaa(this, 0, -1125, 3000, 20); // kentän alalaidassa oleva ruskea maa-alue
        LisaaAarteita(RandomGen.NextInt(4, 10));
    }


    /// <summary>
    /// Luodaan seuraukset tapahtumalle, kun pelaaja osuu mystiseen valkoiseen aukkoon pelimaailmassa.
    /// </summary>
    /// <param name="pelaaja">pelipallo</param>
    /// <param name="tasot">mystinen valkoinen aukko</param>
    private void OsuiTasoon(PhysicsObject pelaaja, PhysicsObject tasot)
    {
        if (keratytAarteet == keratytAarteet.MinValue) // jos aarteet kerätty, niin voidaan vaihtaa maailmaa
        {
            if (tasot == taso)
            {
                switch (maailmaNro)
                {
                    case 1:
                        maailmaNro = 2;
                        break;
                    case 2:
                        maailmaNro = 1;
                        break;
                    case 3:
                        maailmaNro = 2;
                        break;
                }
            }
            if (tasot == taso2)
            {
                switch (maailmaNro)
                {
                    case 2:
                        maailmaNro = 3;
                        break;
                    case 3:
                        maailmaNro = 1;
                        break;
                }
            }
            ClearAll();
            rajoitin = false;
            taso1OlemassaOlo = false;
            taso2OlemassaOlo = false;
            Begin();
        }
        else // jos kaikki aarteita ei ole kerätty, niin näytetään tämä viesti
        {
            MessageDisplay aukko = new MessageDisplay();
            aukko.Add("Osuit mystiseen aukkoon... :O", Color.White);
            aukko.Position = RandomGen.NextVector(100, 100, 600, 600);
            aukko.BackgroundColor = Color.Transparent;
            aukko.BorderColor = Color.Transparent;
            this.Add(aukko);
            pelaaja.Hit(RandomGen.NextVector(1000, 2500, 1000, 2500));
        }
    }


    /// <summary>
    /// Luodaan seuraukset tapahtuma, kun pelaaja osuu vaaralliseen tähteen. Pelaaja tuhoutuu ja kenttä alkaa alusta.
    /// </summary>
    /// <param name="pelaaja">pelipallo</param>
    /// <param name="tasot">mystinen valkoinen aukko</param>
    private void OsuiVaaraan(PhysicsObject pelaaja, PhysicsObject tasot)
    {
        Explosion rajahdys = new Explosion(pelaaja.Width * 2);
        rajahdys.Position = pelaaja.Position;
        this.Add(rajahdys);
        pelaaja.Destroy();
        MessageDisplay vaaraViesti = new MessageDisplay();
        vaaraViesti.Add("Törmäsit vaaralliseen tähteen ja tuhouduit! :(", Color.Red);
        vaaraViesti.Position = new Vector(300, 250);
        vaaraViesti.BackgroundColor = Color.Transparent;
        vaaraViesti.BorderColor = Color.Transparent;
        this.Add(vaaraViesti);
        Timer.SingleShot(2.0,
        delegate { VaaraTapahtuma(); }) ;
    }


    /// <summary>
    /// OsuiVaaraan aliohjelman timerista seuraava singleshot tapahtuma, joka aiheuttaa kentän restartin.
    /// </summary>
    private void VaaraTapahtuma()
    {
        ClearAll();
        rajoitin = false;
        taso1OlemassaOlo = false;
        taso2OlemassaOlo = false;
        Begin();
    }


    /// <summary>
    /// Luodaan vapaana olevia tähtiä varten laskuri.
    /// </summary>
    /// <param name="aarteet">aarteet/tähdet pelissä</param>
    /// <returns>Laskuri, joka näyttää kerättävien aarteiden/tähtien määrän</returns>
    IntMeter LisaaLaskurit(int aarteet)
    {
        keratytAarteet = new IntMeter(0);
        keratytAarteet.Value = aarteet;
        keratytAarteet.MinValue = aarteet - aarteet;
        keratytAarteet.LowerLimit += KaikkiKeratty;
        naytto = new Label();
        naytto.BindTo(keratytAarteet);
        naytto.Position = new Vector(0, 425);
        naytto.TextColor = Color.Yellow;
        naytto.BorderColor = Color.Transparent;
        naytto.Color = Color.Transparent;
        Add(naytto);
        return keratytAarteet;
    }


    /// <summary>
    /// Tapahtuma sille, kun kaikki arteet/tähdet on kerätty.
    /// </summary>
    private void KaikkiKeratty()
    {
        if(maailmaNro == 3) // Näyetään kolmannessa maailmassa pelin läpäisystä onnitteleva viesti
        {
            MessageDisplay kaikkiAarteet = new MessageDisplay();
            kaikkiAarteet.Add("Onneksi olkoon, pääsit pelin läpi! Voit kuitenkin jatkaa pelaamista niin halutessasi", Color.BrightGreen);
            kaikkiAarteet.BackgroundColor = Color.Transparent;
            kaikkiAarteet.Position = new Vector(100, 300);
            this.Add(kaikkiAarteet);
            SoundEffect lapimeni = LoadSoundEffect("jivatma_levelcomplete");
            lapimeni.Play();
            naytto.Destroy();
        }
        else
        {
            MessageDisplay kaikkiAarteet = new MessageDisplay();
            kaikkiAarteet.Add("Kaikki tähdet kerätty!", Color.Yellow);
            kaikkiAarteet.BackgroundColor = Color.Transparent;
            kaikkiAarteet.Position = new Vector(400, 200);
            this.Add(kaikkiAarteet);
            SoundEffect lapimeni = LoadSoundEffect("jivatma_levelcomplete");
            lapimeni.Play();
            naytto.Destroy();
        }
    }


    /// <summary>
    /// Tapahtuma, kun pelaaja törmää aarteeseen/tähteen
    /// </summary>
    /// <param name="pelaaja">pelipallo</param>
    /// <param name="aarre">aarre/tähti</param>
    private void LoysiAarteen(PhysicsObject pelaaja, PhysicsObject aarre)
    {
        Remove(aarre);
        keratytAarteet.Value -= 1;
        SoundEffect loytyi = LoadSoundEffect("inspector_clap"); // musiikki sille, kun pelaaja kerää tähden
        loytyi.Play();
    }


    /// <summary>
    /// Lisätään aarteet/tähdet pelimaailmaan.
    /// </summary>
    /// <param name="montako">aarteiden/tähtien määrä</param>
    private void LisaaAarteita(int montako)
    {
        LisaaLaskurit(montako);

        if (maailmaNro == 1)
        {
            for (int i = 0; i < montako; i++)
            {
                LisaaAarteet();
            }
        }
        if (maailmaNro != 1)
        {
            for (int i = 0; i < montako; i++)
            {
                PhysicsObject aarre = LisaaLiikkuvatAarteet();
                aarre.Hit(new Vector((RandomGen.NextDouble(-1400, 1400)), (RandomGen.NextDouble(-900, 1000)))); // pistetään tähdet liikkeelle
            }
        }
    }


    /// <summary>
    /// Lisätään vaaroja pelimaailmaan.
    /// </summary>
    /// <param name="montako">vaarojen määrä</param>
    private void LisaaVaaroja(int montako)
    {
        for(int i = 0; i < montako; i++)
        {
            PhysicsObject vaara = LisaaVaarat();
        }
    }


    /// <summary>
    /// Perus aarre/tähti, joka pysyy paikoillaan.
    /// </summary>
    /// <returns>Palauttaa staattisen fysiikkaobjektin, joka toimii aarteena/tähtenä.</returns>
    PhysicsObject LisaaAarteet()
    {
        PhysicsObject aarre = PhysicsObject.CreateStaticObject(25, 25, Shape.Rectangle);
        aarre.Position = new Vector((RandomGen.NextDouble(-1400, 1400)),(RandomGen.NextDouble(-900, 1000)));
        aarre.Color = Color.Yellow;
        aarre.IgnoresCollisionResponse = true;
        aarre.Tag = "aarre";
        this.Add(aarre);
        return (aarre);
    }

    
    /// <summary>
    /// Liikkumiseen kykenevä aarre/tähti.
    /// </summary>
    /// <returns>Palauttaa ei staattisen fysiikkaobjektin, joka toimii aarteena/tähtenä.</returns>
    PhysicsObject LisaaLiikkuvatAarteet()
    {
        PhysicsObject aarre = new PhysicsObject(25, 25, Shape.Rectangle);
        aarre.Position = new Vector((RandomGen.NextDouble(-1400, 1400)), (RandomGen.NextDouble(-900, 1000)));
        aarre.Color = Color.Yellow;
        aarre.Tag = "aarre";
        aarre.Restitution = 1;
        aarre.IgnoresGravity = true;
        this.Add(aarre);
        return (aarre);
    }


    /// <summary>
    /// Perus aarre/tähti
    /// </summary>
    /// <returns>Palauttaa staattisen fysiikkaobjektin, joka toimii vaarana.</returns>
    PhysicsObject LisaaVaarat()
    {
        PhysicsObject vaara = PhysicsObject.CreateStaticObject(25, 25, Shape.Rectangle);
        vaara.Position = new Vector((RandomGen.NextDouble(-1400, 1400)), (RandomGen.NextDouble(-900, 1000)));
        vaara.Color = Color.Red;
        vaara.Restitution = 1;
        vaara.IgnoresGravity = true;
        vaara.Tag = "vaara";
        this.Add(vaara);
        return (vaara);
    }


    /// <summary>
    /// Aliohjelma, joka lisää maata muistuttavan tason kenttään.
    /// </summary>
    /// <param name="peli">peli</param>
    /// <param name="x">x-koordinaatti</param>
    /// <param name="y">y-koordinaatti</param>
    /// <param name="leveys">tason leveys</param>
    /// <param name="korkeus">tason korkeus</param>
    /// <returns>Palauttaa saattisen fysiikkaobjektin, joka toimii kentan maaosana</returns>
    PhysicsObject LisaaMaa(Game peli, double x, double y, double leveys, double korkeus)
    {
        PhysicsObject lituska = PhysicsObject.CreateStaticObject(leveys, korkeus, Shape.Rectangle);
        lituska.Position = new Vector(x, y);
        lituska.Color = Color.DarkBrown;
        peli.Add(lituska);
        return(lituska);
    }


    /// <summary>
    /// Tässä luodaan pelaajalle pelattava hahmo, joka on pallo.
    /// </summary>
    /// <param name="peli">peli</param>
    /// <param name="x">x-koordinaatti</param>
    /// <param name="y">-y-koordinaatti</param>
    /// <param name="sade">pallon säde</param>
    /// <returns>Palauttaa pallon, joka toimii pelaajan hahmona</returns>
    PhysicsObject Pelaaja(Game peli, double x, double y, double sade)
    {
        PhysicsObject pallo = new PhysicsObject(sade, sade, Shape.Circle);
        pallo.Position = new Vector(x, y);
        pallo.Color = Color.Yellow;
        pallo.Restitution = 0.5;
        peli.Add(pallo);
        return pallo;
    }


    /// <summary>
    /// Tässä luodaan mystinen valkoinen aukko pelikenttään.
    /// </summary>
    /// <param name="peli">peli</param>
    /// <param name="sade">aukon säde</param>
    /// <param name="x">x-koordinaatti</param>
    /// <param name="y">y-koordinaatti</param>
    /// <returns>Palauttaa valkoisen aukon pelikenttään</returns>
    PhysicsObject UusiTaso(Game peli, double sade, double x, double y)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(sade, sade, Shape.Circle);
        taso.Position = new Vector(x, y);
        taso.Color = Color.White;
        Add(taso);
        return(taso);
    }


    /// <summary>
    /// Pelin ohjaimina toimivat nuolinappaimet ja koyden ampumiseen kaytetaan space nappainta
    /// </summary>
    private void Ohjaimet()
    {
        MessageDisplay.TextColor = Color.White;
        MessageDisplay.BackgroundColor = Color.Transparent;
        Keyboard.Listen(Key.Right, ButtonState.Down, LiikutaPalloa, "Liiku oikealle", pelaaja, new Vector(1500, 0));
        Keyboard.Listen(Key.Left, ButtonState.Down, LiikutaPalloa, "Liiku vasemmalle", pelaaja, new Vector(-1500, 0));
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppy, "Hyppää", pelaaja, new Vector(0, 850));
        Keyboard.Listen(Key.Space, ButtonState.Pressed, AmmuKoysi, "Laukaise köysi ja pidä pohjassa roikkuaksesi köydessä");
        Keyboard.Listen(Key.Space, ButtonState.Released, TuhoaKoysi, "Tuhoa koysi vapauttamalla space");
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }


    /// <summary> 
    /// Aliohjelma, joka liikuttaa pelipalloa
    /// </summary>
    /// <param name="pallo">pelaajapallo</param>
    /// <param name="suunta">suunta, johon liikutetaan</param>
    private void LiikutaPalloa(PhysicsObject pallo, Vector suunta)
    {
        pallo.Push(suunta);
    }


    /// <summary>
    /// Hyppy, joka on rajoitettu toimimaan enintään kahden sekunnin välein.
    /// </summary>
    /// <param name="pallo">pelaajapallo</param>
    /// <param name="suunta">suunta, johon hypätään</param>
    private void Hyppy(PhysicsObject pallo, Vector suunta)
    {
        if(rajoitin == false)
        {
            pallo.Hit(suunta);
            rajoitin = true;
            Timer.SingleShot(2.0,
                delegate { rajoitin = false; }
                );
        }        
    }


    /// <summary>
    /// Luo köyden ja laskee sen pituuden, kulman ja sijainnin.
    /// </summary>
    /// <param name="taso">mystinen valkoinen aukko pelimaailmassa</param>
    /// <returns>Palauttaa koyden</returns>
    PhysicsObject Koysi(PhysicsObject taso)
    {
        double hypotenuusa = Vector.Distance(pelaaja.Position, taso.Position); // kulman laskemista varten hypotenuusa
        double koydenPituus = valitsePituus(hypotenuusa, taso); // valitaan köydelle sopiva pituus
        double viereinenKateetti = (taso.X - pelaaja.X); // kateetti köyden kulman laskemista varten
        double kulmaRad = System.Math.Asin(viereinenKateetti / hypotenuusa); // lasketaan köyden kulma radiaaneina
        double kulmaDeg = kulmaRad * (-180) / System.Math.PI; // muutetaan radiaanit asteiksi
        koysi = new PhysicsObject(2, koydenPituus, Shape.Rectangle);
        koysi.Position = new Vector(((taso.X + pelaaja.X) * 0.5), ((taso.Y + pelaaja.Y) * 0.5));
        koysi.Angle = Angle.FromDegrees(kulmaDeg);
        koysi.Color = Color.White;
        this.Add(koysi);
        return koysi;
    }


    /// <summary>
    /// Valitaan köyden palalle sopiva pituus
    /// </summary>
    /// <param name="hypotenuusa">pelaajan ja valkoisen aukon välinen matka</param>
    /// <param name="taso">valkoinen aukko</param>
    /// <returns>Palauttaa tilanteeseen sopivan koyden pituuden</returns>
    private double valitsePituus(double hypotenuusa, PhysicsObject taso)
    {
        if((hypotenuusa < 900) && (hypotenuusa >= 800)) return Vector.Distance(pelaaja.Position, taso.Position) * 0.80;
        else if((hypotenuusa < 800) && (hypotenuusa >= 700)) return Vector.Distance(pelaaja.Position, taso.Position) * 0.77;
        else if((hypotenuusa < 700) && (hypotenuusa >= 600)) return Vector.Distance(pelaaja.Position, taso.Position) * 0.74;
        else if((hypotenuusa < 600) && (hypotenuusa >= 500)) return Vector.Distance(pelaaja.Position, taso.Position) * 0.71;
        else if((hypotenuusa < 500) && (hypotenuusa >= 400)) return Vector.Distance(pelaaja.Position, taso.Position) * 0.68;
        else if((hypotenuusa < 400) && (hypotenuusa >= 300)) return Vector.Distance(pelaaja.Position, taso.Position) * 0.50;
        else if((hypotenuusa < 300) && (hypotenuusa >= 200)) return Vector.Distance(pelaaja.Position, taso.Position) * 0.35;
        else if((hypotenuusa < 200) && (hypotenuusa >= 100)) return Vector.Distance(pelaaja.Position, taso.Position) * 0.20;
        else if(hypotenuusa < 100) return Vector.Distance(pelaaja.Position, taso.Position) * 0.15;
        else return Vector.Distance(pelaaja.Position, taso.Position) * 0.85;
    }


    /// <summary>
    /// Ammutaan köysi.
    /// </summary>
    private void AmmuKoysi()
    {
        // Kun on olemassa kaksi valkoista aukkoa, niin tarkistetaan onko pelaaja lähempänä toista aukkoa (taso2).
        if ((taso2OlemassaOlo == true) && (Vector.Distance(pelaaja.Position, taso.Position) > Vector.Distance(pelaaja.Position, taso2.Position)))
        {
            if (pelaaja.Y > taso2.Y) return; // myös jos pelaaja on aukon yläpuolella, niin köyttä ei tehdä
            double pelaajanX = pelaaja.X;
            double tasonVasen = taso2.Left;
            double tasonOikea = taso2.Right;
            koysi = Koysi(taso2);
            AxleJoint koysiJaTaso = new AxleJoint(koysi, taso2); // axlejointilla tämä liitos, jotta valkoinen aukko ei menetä staattisen objektin ominaisuuksia
            Add(koysiJaTaso);
            PhysicsStructure pelaajaJaKoysi = new PhysicsStructure(pelaaja, koysi); // tämä onnistuu physicsstructurella, sillä kummatkaan ei ole staattisia objekteja
            pelaajaJaKoysi.Softness = 3; // luo vaikutelmaa, että pallo todella roikkuisi köydessä
            koysi.IgnoresCollisionResponse = true;
            koydenOlemassaOlo = true; // kerrotaan että köysi on nyt olemassa, jotta uutta köyttä ei voi enää luoda
        }
        // Muuten tarkistetaan vain onko ensimmäinen valkoinen aukko olemassa ja ettei köyttä jo ole ammuttu edellisessä kohdassa.
        if ((taso1OlemassaOlo == true) && (koydenOlemassaOlo == false))
        {
            if (pelaaja.Y > taso.Y) return;
            double pelaajanX = pelaaja.X;
            double tasonVasen = taso.Left;
            double tasonOikea = taso.Right;
            koysi = Koysi(taso);
            AxleJoint koysiJaTaso = new AxleJoint(koysi, taso); // axlejointilla tämä liitos, jotta valkoinen aukko ei menetä staattisen objektin ominaisuuksia
            Add(koysiJaTaso);
            PhysicsStructure pelaajaJaKoysi = new PhysicsStructure(pelaaja, koysi); // tämä onnistuu physicsstructurella, sillä kummatkaan ei ole staattisia objekteja
            pelaajaJaKoysi.Softness = 3; // luo vaikutelmaa, että pallo todella roikkuisi köydessä
            koysi.IgnoresCollisionResponse = true;
            koydenOlemassaOlo = true; // kerrotaan että köysi on nyt olemassa, jotta uutta köyttä ei voi enää luoda
        }
    }


    /// <summary>
    /// Tuhotaan köysi, kun pelaaja on vapauttanut space näppäimen
    /// </summary>
    private void TuhoaKoysi()
    {
        if(koydenOlemassaOlo == true) // tarkistetaan että köysi on olemassa ennen sen tuhoamista.
        {
            koysi.Destroy();
            koydenOlemassaOlo = false;
        }
    }


}
