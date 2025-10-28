"use client";
import Image from "next/image";
import { useRef } from "react";

import gsap from "gsap";
import { useGSAP } from "@gsap/react";
import { ScrambleTextPlugin } from "gsap/ScrambleTextPlugin";
import { SplitText } from "gsap/SplitText";
import { MotionPathPlugin } from "gsap/MotionPathPlugin";
import CustomEase from "gsap/CustomEase";

gsap.registerPlugin(ScrambleTextPlugin, SplitText);
gsap.registerPlugin(useGSAP);
gsap.registerPlugin(MotionPathPlugin);
gsap.registerPlugin(CustomEase);

export default function StartPart() {
  const text = useRef<HTMLParagraphElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  useGSAP(
    () => {
      if (text.current && containerRef.current) {
        const split = SplitText.create(text.current, { type: "words" });
        const rectCon = containerRef.current.getBoundingClientRect();

        split.words.forEach((word, i) => {
          const startX = Math.random() * rectCon.width - rectCon.width / 2;
          const startY = rectCon.height / 3;

          gsap.set(word, { x: startX, y: startY, opacity: 0 });

          const control1 = { x: startX + 20, y: startY };
          const control2 = { x: 0 + 20, y: 0 - 40 };

          gsap.to(word, {
            duration: 1,
            motionPath: {
              path: [
                { x: startX, y: startY },
                control1,
                control2,
                { x: 0, y: 0 },
              ],
              curviness: 1.25,
            },

            ease: CustomEase.create(
              "custom",
              "M0,0 C0.027,0.123 0.118,0.192 0.17,0.294 0.327,0.6 0.512,0.851 0.608,0.946 0.7,1.038 0.851,1 1,1 "
            ),
            opacity: 1,
            delay: i * 0.1, // stagger
          });
        });
      }
    },
    { scope: text }
  );

  return (
    <div
      ref={containerRef}
      className="w-[700px] h-[90vh] flex gap-10 flex-col pt-10"
    >
      <Image
        src="https://media1.giphy.com/media/v1.Y2lkPTc5MGI3NjExczBsYzQweGU1MzRrNGJzbTlxYWIxc2VqcDRyNDg2eTFyNDBtbjh4byZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/lxsQ9dLbw1kTSVS9rt/giphy.gif"
        alt="some txt"
        width={225}
        height={225}
        className="self-center"
      />
      <p ref={text} className="text-3xl font-bold text-center">
        TodoHub: A lightweight practical application for CRUD operations on
        To-Do with modern architecture.
      </p>
      <p className="text-shadow-md text-left">
        - Front-End Stack: Next.js, axios, zxcvbn, react-syntax-highlighter,
        GSAP
        <br />
        - Back-End Stack: .NET Core Web Api, Entity Framework, PostqreSQL
        Database, JWT, Redis, RabbitMQ
        <br />- FEATURE: improve the Front-End structure, write unit tests, log
        the project, more animations
      </p>
    </div>
  );
}
