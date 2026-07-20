using System.Collections.Generic;
using UnityEngine;

namespace ARDiabetes
{
    public partial class AppBootstrap
    {
        // ============================================================
        // Contenido de los 3 libros
        // ============================================================
        void BuildBooks()
        {
            AudioClip N(int i) => (narracion != null && i < narracion.Length) ? narracion[i] : null;

            books = new BookDef[3];
            books[0] = new BookDef
            {
                Title = "Libro Fisiológico", Accent = UIKit.Fisio, HeroIcon = spPancreas,
                Model = pancreasModel, ModelTint = UIKit.Hex("C86B6B"),
                TopicTitle = new[] { "¿Qué es la diabetes tipo 1?", "El páncreas", "Insulina y glucosa", "Cómo funciona" },
                TopicSub = new[] { "Conoce la enfermedad", "El órgano de la insulina", "Cómo obtienes energía", "Todo trabaja en equipo" },
                DescKids = new[]
                {
                    "Tu cuerpo tiene un ayudante llamado insulina. En la diabetes tipo 1, ese ayudante casi no aparece. ¡Vamos a conocerlo juntos!",
                    "El páncreas es un amigo escondido en tu barriga. Fabrica la insulina. ¡Míralo en 3D!",
                    "La insulina es como una llave mágica. Abre la puerta de tus células para que entre la energía (glucosa).",
                    "El páncreas, la insulina y la glucosa trabajan en equipo, como superhéroes, para darte energía todos los días."
                },
                DescPre = new[]
                {
                    "La diabetes tipo 1 aparece cuando el cuerpo casi no produce insulina, la hormona que nos da energía. ¡Aprende a conocerla y cuidarte!",
                    "El páncreas es el órgano que fabrica la insulina en tu cuerpo. Explóralo en 3D y descubre cómo es por dentro.",
                    "La insulina es como una llave que deja entrar la glucosa (energía) a tus células para que funcionen.",
                    "Descubre cómo trabajan juntos el páncreas, la insulina y la glucosa para darte energía cada día."
                },
                DescTeens = new[]
                {
                    "La diabetes tipo 1 es una enfermedad autoinmune: el sistema de defensas del cuerpo ataca por error las células del páncreas que producen insulina, la hormona clave para usar la glucosa como energía.",
                    "El páncreas es una glándula mixta: sus células beta, en los islotes de Langerhans, producen insulina, la hormona que regula tu glucosa en sangre.",
                    "La insulina actúa como una llave molecular: se une a receptores en la membrana celular y permite el transporte de glucosa al interior para producir energía.",
                    "El eje páncreas-insulina-glucosa regula tu metabolismo energético: sin suficiente insulina, la glucosa se acumula en la sangre en vez de entrar en las células."
                },
                TopicIcon = new[] { icQuestion, icPancreas, icDrop, icGear },
                Markers = markersFisio,
                Narration = new[] { N(0), N(1), N(2), N(3) },
                Quiz = new[]
                {
                    new GameItem { Type = GameType.Matching, Q = "Relaciona cada célula con lo que le corresponde:",
                        Left = new[] { "Célula beta", "Célula alfa", "Célula delta" },
                        Right = new[] { "Produce insulina", "Produce glucagón", "Mantiene el equilibrio entre las otras dos" } },
                    new GameItem { Q = "Cuando la glucosa entra a la célula, se transforma en _______, que es la energía que la célula realmente puede usar para hacer su trabajo.",
                        Options = new[] { "ATP", "glucógeno", "insulina", "agua" }, Correct = 0 },
                    new GameItem { Q = "¿Qué le permite a la insulina ayudar a que la glucosa entre a la célula?",
                        Options = new[] {
                            "Abre la célula físicamente, como una llave que abre una cerradura",
                            "Se une a un receptor en la célula y activa una señal interna que hace que la propia célula abra su puerta",
                            "Viaja dentro de la glucosa y la empuja hacia adentro",
                            "No hace nada, la glucosa entra sola" }, Correct = 1 },
                },
            };
            books[1] = new BookDef
            {
                Title = "Libro Nutricional", Accent = UIKit.Nutri, HeroIcon = spIconNutri,
                Model = platoModel, ModelTint = UIKit.Hex("7CB86F"),
                TopicTitle = new[] { "Plato saludable", "Carbohidratos", "Alimentos recomendados", "Hábitos saludables" },
                TopicSub = new[] { "Equilibra tu comida", "La energía más rápida", "Elige mejor", "Cuídate cada día" },
                DescKids = new[]
                {
                    "Un plato feliz tiene la mitad de verduras, un poco de proteína y un poco de carbohidratos. ¡Así tu cuerpo está contento!",
                    "Los carbohidratos son la comida que sube tu energía más rápido, como el pan, el arroz y las frutas.",
                    "Las verduras y las proteínas te ayudan a mantenerte fuerte y con energía estable todo el día.",
                    "Comer a la misma hora, tomar agua y no comer tantos dulces hace que cuidarte sea más fácil."
                },
                DescPre = new[]
                {
                    "El plato saludable te ayuda a equilibrar tu comida: mitad de vegetales, un cuarto de proteína y un cuarto de carbohidratos. Así tu glucosa sube de forma más estable.",
                    "Los carbohidratos son la energía que más rápido sube tu glucosa. Aprende a reconocerlos en el pan, el arroz, la pasta y las frutas.",
                    "Las verduras, las proteínas magras y las grasas saludables te ayudan a mantener tu glucosa estable durante más tiempo.",
                    "Comer a las mismas horas, tomar agua y evitar azúcares en exceso hacen que tu diabetes sea mucho más fácil de controlar cada día."
                },
                DescTeens = new[]
                {
                    "El método del plato ayuda a controlar la carga glucémica de tus comidas: 50% vegetales, 25% proteína y 25% carbohidratos, favoreciendo una curva de glucosa más estable.",
                    "Los carbohidratos tienen el mayor impacto en la glucemia postprandial. Conocer su índice glucémico te ayuda a anticipar y ajustar tu dosis de insulina.",
                    "Priorizar fibra, proteínas magras y grasas saludables enlentece la absorción de glucosa, reduciendo picos glucémicos bruscos.",
                    "Mantener horarios regulares de comida, buena hidratación y control de azúcares simples mejora la variabilidad glucémica a largo plazo."
                },
                TopicIcon = new[] { icPlate, icBread, icApple, icClock },
                Markers = markersNutri,
                Narration = new[] { N(4), N(5), N(6), N(7) },
                Quiz = new[]
                {
                    new GameItem { Type = GameType.Matching, Q = "Relaciona cada alimento con su grupo correspondiente:",
                        Left = new[] { "Pollo", "Manzana", "Arroz", "Yogur", "Palta" },
                        Right = new[] { "Proteínas", "Frutas y verduras", "Cereales", "Lácteos", "Grasas saludables" } },
                    new GameItem { Type = GameType.MultiSelect, Q = "Selecciona los alimentos que contienen carbohidratos:",
                        Options = new[] { "Pan", "Pollo", "Plátano", "Huevo", "Leche", "Queso" },
                        CorrectMask = new[] { true, false, true, false, false, false } },
                    new GameItem { Q = "Verdadero o falso: los alimentos nos dan energía para movernos, crecer y pensar.",
                        Options = new[] { "Verdadero", "Falso" }, Correct = 0 },
                },
            };
            books[2] = new BookDef
            {
                Title = "Libro Clínico", Accent = UIKit.Clin, HeroIcon = spIconClinico,
                Model = glucoModel, ModelTint = UIKit.Hex("4E8FD1"),
                TopicTitle = new[] { "Uso de insulina", "Síntomas", "Monitoreo de glucosa", "Cuidados diarios" },
                TopicSub = new[] { "Cómo aplicarla", "Reconócelos a tiempo", "Mide y controla", "Rutina de cuidado" },
                DescKids = new[]
                {
                    "La insulina se aplica con una inyectorcita o una bombita, antes de comer, para ayudarte a usar la energía de la comida.",
                    "Tener mucha sed, ir mucho al baño y sentirte cansado puede avisarte que algo anda diferente.",
                    "Medir tu glucosa con el glucómetro te ayuda a saber cómo está tu cuerpo, como revisar la energía de un videojuego.",
                    "Revisar tus piecitos, guardar bien tu insulina en frío y llevar algo dulce por si acaso son tus superpoderes de cuidado."
                },
                DescPre = new[]
                {
                    "La insulina se aplica con una inyección o una bomba, generalmente antes de comer, para ayudar a que la glucosa entre a tus células.",
                    "Mucha sed, ganas frecuentes de ir al baño y cansancio pueden indicar que tu glucosa está muy alta o muy baja.",
                    "Medir tu glucosa con el glucómetro varias veces al día te ayuda a saber cómo reacciona tu cuerpo a la comida y al ejercicio.",
                    "Revisar tus pies, guardar bien tu insulina en frío y llevar siempre algo dulce por si tu glucosa baja son hábitos que cuidan tu salud."
                },
                DescTeens = new[]
                {
                    "La insulina se administra por vía subcutánea (pluma, jeringa o bomba), habitualmente antes de las comidas, ajustando la dosis según los carbohidratos y la glucosa previa.",
                    "Poliuria, polidipsia y fatiga son signos clásicos de hiperglucemia; temblor, sudoración y confusión pueden indicar hipoglucemia — ambos requieren atención inmediata.",
                    "El automonitoreo (glucómetro o sensor continuo) permite identificar patrones glucémicos y ajustar decisiones de insulina, alimentación y actividad física.",
                    "La inspección diaria de los pies, la conservación adecuada de la insulina en frío y llevar una fuente rápida de glucosa son hábitos clave para prevenir complicaciones."
                },
                TopicIcon = new[] { icSyringe, icAlert, icDrop, icCalendar },
                Markers = markersClinico,
                Narration = new[] { N(8), N(9), N(10), N(11) },
                Quiz = new[]
                {
                    new GameItem { Q = "¿Qué necesito para medir mi glucosa?",
                        Options = new[] { "Un lápiz", "Un glucómetro y una tira reactiva", "Un termómetro" }, Correct = 1 },
                    new GameItem { Q = "¿Para qué sirve la insulina?",
                        Options = new[] { "Para que la glucosa entre en las células", "Para cambiar el color de la sangre", "Para medir la temperatura" }, Correct = 0 },
                    new GameItem { Q = "¿Cuál de estos hábitos ayuda a cuidar tu salud?",
                        Options = new[] { "Dormir bien y comer saludable", "Comer solo dulces", "No tomar agua" }, Correct = 0 },
                },
            };
        }
    }
}
